using NexaERP.Application.DTOs.SalesOrders;

namespace NexaERP.Application.Interfaces;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderResponse>> GetAllAsync();
    Task<SalesOrderResponse> GetByIdAsync(Guid id);
    Task<SalesOrderResponse> CreateAsync(CreateSalesOrderRequest request, Guid createdByUserId);
    Task<SalesOrderResponse> CancelAsync(Guid id, Guid cancelledByUserId);
}
