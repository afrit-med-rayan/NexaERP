using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.DTOs.Products;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _products.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _products.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _products.CreateAsync(request);
        return Created($"api/products/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        => Ok(await _products.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _products.DeleteAsync(id);
        return NoContent();
    }
}
