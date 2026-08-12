using NexaERP.Domain.Enums;

namespace NexaERP.Domain.Entities;

/// <summary>
/// Append-only ledger of every stock change. Never updated or deleted.
/// Current Inventory.Quantity is kept in sync inside the same transaction
/// that inserts this row.
/// </summary>
public class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public int WarehouseId { get; set; }
    public MovementType MovementType { get; set; }

    /// <summary>
    /// Positive = stock in, negative = stock out.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Optional link to the SalesOrder or PurchaseOrder that caused this movement.
    /// </summary>
    public Guid? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedById { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
