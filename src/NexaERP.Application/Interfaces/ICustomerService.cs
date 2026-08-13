using NexaERP.Application.DTOs.Customers;

namespace NexaERP.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
}
