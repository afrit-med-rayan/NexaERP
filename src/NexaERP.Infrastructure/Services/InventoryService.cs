using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.Inventory;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Enums;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<InventoryResponse>> GetAllAsync()
    {
        return await _db.Inventories
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.Warehouse)
            .OrderBy(i => i.Product.Name).ThenBy(i => i.Warehouse.Name)
            .Select(i => MapToResponse(i))
            .ToListAsync();
    }

    public async Task<IEnumerable<LowStockProductResponse>> GetLowStockAsync(int? warehouseId)
    {
        // Calls sp_GetLowStockProducts stored procedure
        var warehouseParam = warehouseId.HasValue
            ? new SqlParameter("@WarehouseId", warehouseId.Value)
            : new SqlParameter("@WarehouseId", DBNull.Value);

        var results = await _db.Database
            .SqlQuery<LowStockRaw>(
                $"EXEC sp_GetLowStockProducts @WarehouseId = {warehouseParam}")
            .ToListAsync();

        return results.Select(r => new LowStockProductResponse(
            r.ProductId, r.Sku, r.Name, r.CategoryName,
            r.WarehouseId, r.WarehouseName, r.Quantity, r.ReorderThreshold));
    }

    public async Task<InventoryResponse> AdjustAsync(AdjustInventoryRequest request, Guid performedByUserId)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId
                                   && i.WarehouseId == request.WarehouseId)
            ?? throw new NotFoundException(
                $"No inventory record found for product '{request.ProductId}' in warehouse '{request.WarehouseId}'.");

        var newQty = inventory.Quantity + request.QuantityDelta;
        if (newQty < 0)
            throw new BusinessException(
                $"Adjustment would bring quantity to {newQty}. Inventory cannot go negative.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            inventory.Quantity = newQty;
            _db.StockMovements.Add(new StockMovement
            {
                ProductId    = request.ProductId,
                WarehouseId  = request.WarehouseId,
                MovementType = MovementType.Adjustment,
                Quantity     = request.QuantityDelta,
                CreatedById  = performedByUserId
            });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return MapToResponse(inventory);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static InventoryResponse MapToResponse(Inventory i) =>
        new(i.Id,
            i.ProductId, i.Product?.Name ?? string.Empty, i.Product?.Sku ?? string.Empty,
            i.WarehouseId, i.Warehouse?.Name ?? string.Empty,
            i.Quantity,
            i.Product?.ReorderThreshold ?? 0,
            i.Quantity <= (i.Product?.ReorderThreshold ?? 0));

    // Projection type for raw SQL result from sp_GetLowStockProducts
    private sealed class LowStockRaw
    {
        public Guid ProductId      { get; set; }
        public string Sku          { get; set; } = string.Empty;
        public string Name         { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int WarehouseId     { get; set; }
        public string WarehouseName{ get; set; } = string.Empty;
        public int Quantity        { get; set; }
        public int ReorderThreshold{ get; set; }
    }
}
