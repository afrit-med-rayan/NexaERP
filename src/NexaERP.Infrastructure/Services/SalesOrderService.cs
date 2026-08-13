using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.SalesOrders;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Enums;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly AppDbContext _db;
    public SalesOrderService(AppDbContext db) => _db = db;

    // ── Query ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<SalesOrderResponse>> GetAllAsync()
    {
        var orders = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.CreatedBy)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToResponse);
    }

    public async Task<SalesOrderResponse> GetByIdAsync(Guid id)
    {
        var order = await LoadOrderAsync(id);
        return MapToResponse(order);
    }

    // ── Create (centrepiece) ───────────────────────────────────────────────────

    /// <summary>
    /// Business rules enforced:
    ///  1. All items validated for stock BEFORE any write.
    ///  2. IsActive = false products rejected.
    ///  3. Inventory decremented + StockMovement inserted in one DB transaction.
    ///  4. Any failure rolls back the entire order.
    /// </summary>
    public async Task<SalesOrderResponse> CreateAsync(
        CreateSalesOrderRequest request, Guid createdByUserId)
    {
        // -- Pre-flight validation (outside transaction — fail fast) -----------

        if (!await _db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            throw new NotFoundException($"Customer '{request.CustomerId}' not found.");

        var itemList = request.Items.ToList();
        if (!itemList.Any())
            throw new BusinessException("A sales order must have at least one item.");

        // Load all required products + inventory in two queries
        var productIds = itemList.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var inventories = await _db.Inventories
            .Where(inv => productIds.Contains(inv.ProductId)
                       && inv.WarehouseId == request.WarehouseId)
            .ToDictionaryAsync(inv => inv.ProductId);

        // Validate every line item
        foreach (var item in itemList)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException($"Product '{item.ProductId}' not found.");

            if (!product.IsActive)
                throw new BusinessException(
                    $"Product '{product.Name}' is inactive and cannot be ordered.");

            if (item.Quantity <= 0)
                throw new BusinessException(
                    $"Quantity for product '{product.Name}' must be greater than zero.");

            if (!inventories.TryGetValue(item.ProductId, out var inv)
                || inv.Quantity < item.Quantity)
            {
                var available = inventories.TryGetValue(item.ProductId, out var inv2) ? inv2.Quantity : 0;
                throw new BusinessException(
                    $"Insufficient stock for '{product.Name}': requested {item.Quantity}, available {available}.");
            }
        }

        // -- Transactional write -----------------------------------------------
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = new SalesOrder
            {
                CustomerId  = request.CustomerId,
                CreatedById = createdByUserId,
                Status      = OrderStatus.Completed,
                OrderDate   = DateTime.UtcNow
            };
            _db.SalesOrders.Add(order);

            foreach (var item in itemList)
            {
                var product = products[item.ProductId];
                var inv     = inventories[item.ProductId];

                // Add line item (snapshot price at time of sale)
                order.Items.Add(new SalesOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity  = item.Quantity,
                    UnitPrice = product.Price
                });

                // Decrement stock
                inv.Quantity -= item.Quantity;

                // Append-only stock movement (Sale = negative quantity)
                _db.StockMovements.Add(new StockMovement
                {
                    ProductId    = item.ProductId,
                    WarehouseId  = request.WarehouseId,
                    MovementType = MovementType.Sale,
                    Quantity     = -item.Quantity,
                    ReferenceId  = order.Id,
                    CreatedById  = createdByUserId
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Cancelling a Completed order:
    ///  - Restores inventory for every line item.
    ///  - Inserts a compensating StockMovement (Adjustment, positive qty).
    ///  - All in a single DB transaction.
    /// </summary>
    public async Task<SalesOrderResponse> CancelAsync(Guid id, Guid cancelledByUserId)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Sales order '{id}' not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessException("Order is already cancelled.");

        if (order.Status != OrderStatus.Completed)
            throw new BusinessException("Only Completed orders can be cancelled.");

        // Determine which warehouse the original movements used
        var warehouseId = await _db.StockMovements
            .Where(sm => sm.ReferenceId == order.Id && sm.MovementType == MovementType.Sale)
            .Select(sm => sm.WarehouseId)
            .FirstOrDefaultAsync();

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Cancelled;

            foreach (var item in order.Items)
            {
                // Restore inventory
                var inv = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId
                                           && i.WarehouseId == warehouseId);
                if (inv != null)
                    inv.Quantity += item.Quantity;

                // Compensating movement (Adjustment, positive = stock in)
                _db.StockMovements.Add(new StockMovement
                {
                    ProductId    = item.ProductId,
                    WarehouseId  = warehouseId,
                    MovementType = MovementType.Adjustment,
                    Quantity     = item.Quantity,
                    ReferenceId  = order.Id,
                    CreatedById  = cancelledByUserId
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await GetByIdAsync(id);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<SalesOrder> LoadOrderAsync(Guid id) =>
        await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.CreatedBy)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
        ?? throw new NotFoundException($"Sales order '{id}' not found.");

    private static SalesOrderResponse MapToResponse(SalesOrder o)
    {
        var items = o.Items.Select(i => new SalesOrderItemResponse(
            i.Id,
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Sku  ?? string.Empty,
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice)).ToList();

        return new SalesOrderResponse(
            o.Id,
            o.CustomerId,
            o.Customer?.Name ?? string.Empty,
            o.OrderDate,
            o.Status.ToString(),
            o.CreatedBy?.FullName ?? string.Empty,
            items,
            items.Sum(i => i.LineTotal));
    }
}
