namespace NexaERP.Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }

    // Navigation
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
