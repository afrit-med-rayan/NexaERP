using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.DTOs.SalesOrders;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _orders;
    public SalesOrdersController(ISalesOrderService orders) => _orders = orders;

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,SalesEmployee,Accountant")]
    public async Task<IActionResult> GetAll() => Ok(await _orders.GetAllAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,SalesEmployee,Accountant")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _orders.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,SalesEmployee")]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _orders.CreateAsync(request, userId);
        return Created($"api/sales-orders/{result.Id}", result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _orders.CancelAsync(id, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
}
