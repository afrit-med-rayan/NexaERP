using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.Customers;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    public CustomerService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
    {
        return await _db.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerResponse(c.Id, c.Name, c.Email, c.Phone, c.Address, c.CreatedAt))
            .ToListAsync();
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name    = request.Name,
            Email   = request.Email,
            Phone   = request.Phone,
            Address = request.Address
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return new CustomerResponse(customer.Id, customer.Name, customer.Email,
            customer.Phone, customer.Address, customer.CreatedAt);
    }
}
