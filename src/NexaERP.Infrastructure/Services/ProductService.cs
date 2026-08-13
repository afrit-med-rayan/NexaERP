using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.Products;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        return await _db.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => MapToResponse(p))
            .ToListAsync();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Product '{id}' not found.");
        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku))
            throw new BusinessException($"SKU '{request.Sku}' already exists.");

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        var product = new Product
        {
            Sku              = request.Sku,
            Name             = request.Name,
            Description      = request.Description,
            CategoryId       = request.CategoryId,
            Price            = request.Price,
            CostPrice        = request.CostPrice,
            ReorderThreshold = request.ReorderThreshold
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException($"Product '{id}' not found.");

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        product.Name             = request.Name;
        product.Description      = request.Description;
        product.CategoryId       = request.CategoryId;
        product.Price            = request.Price;
        product.CostPrice        = request.CostPrice;
        product.ReorderThreshold = request.ReorderThreshold;
        product.IsActive         = request.IsActive;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException($"Product '{id}' not found.");

        // Soft-delete: mark inactive rather than removing
        product.IsActive = false;
        await _db.SaveChangesAsync();
    }

    private static ProductResponse MapToResponse(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.CategoryId,
            p.Category?.Name ?? string.Empty,
            p.Price, p.CostPrice, p.ReorderThreshold, p.IsActive, p.CreatedAt);
}
