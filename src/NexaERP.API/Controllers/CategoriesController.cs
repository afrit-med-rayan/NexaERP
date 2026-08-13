using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.DTOs.Products;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;
    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _categories.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _categories.CreateAsync(request);
        return Created($"api/categories/{result.Id}", result);
    }
}
