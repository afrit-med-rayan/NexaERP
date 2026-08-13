using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.DTOs.Customers;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Manager,SalesEmployee,Accountant")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    public CustomersController(ICustomerService customers) => _customers = customers;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _customers.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,SalesEmployee")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var result = await _customers.CreateAsync(request);
        return Created($"api/customers/{result.Id}", result);
    }
}
