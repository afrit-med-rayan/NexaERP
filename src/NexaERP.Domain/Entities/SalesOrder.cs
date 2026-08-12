using NexaERP.Domain.Enums;

namespace NexaERP.Domain.Entities;

public class SalesOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Guid CreatedById { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
