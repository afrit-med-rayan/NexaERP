using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.DTOs.Inventory;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,Manager,WarehouseEmployee")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;
    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    /// <summary>List all inventory records (product + warehouse + quantity).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _inventory.GetAllAsync());

    /// <summary>Products below their reorder threshold. Optionally filter by warehouseId.</summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int? warehouseId)
        => Ok(await _inventory.GetLowStockAsync(warehouseId));

    /// <summary>Apply a stock adjustment (positive = in, negative = out).</summary>
    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,WarehouseEmployee")]
    public async Task<IActionResult> Adjust([FromBody] AdjustInventoryRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
        var result = await _inventory.AdjustAsync(request, userId);
        return Ok(result);
    }
}
