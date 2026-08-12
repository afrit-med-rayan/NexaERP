namespace NexaERP.Domain.Entities;

public class SalesOrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Price snapshot at the time of sale — intentionally not a live FK to Product.Price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    // Navigation
    public SalesOrder SalesOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
