using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.Products;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return new CategoryResponse(category.Id, category.Name, category.Description);
    }
}
