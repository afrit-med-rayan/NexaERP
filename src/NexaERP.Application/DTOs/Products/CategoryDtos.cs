namespace NexaERP.Application.DTOs.Products;

public record CategoryResponse(int Id, string Name, string? Description);

public record CreateCategoryRequest(string Name, string? Description);
