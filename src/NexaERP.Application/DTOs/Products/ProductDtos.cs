namespace NexaERP.Application.DTOs.Products;

public record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    decimal Price,
    decimal CostPrice,
    int ReorderThreshold,
    bool IsActive,
    DateTime CreatedAt);

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    int CategoryId,
    decimal Price,
    decimal CostPrice,
    int ReorderThreshold);

public record UpdateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    decimal Price,
    decimal CostPrice,
    int ReorderThreshold,
    bool IsActive);
