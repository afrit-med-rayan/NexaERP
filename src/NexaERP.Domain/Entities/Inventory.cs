namespace NexaERP.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public Guid ProductId { get; set; }
    public int WarehouseId { get; set; }

    /// <summary>
    /// Current stock level. Never negative — enforced by CHECK constraint AND application layer.
    /// </summary>
    public int Quantity { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
