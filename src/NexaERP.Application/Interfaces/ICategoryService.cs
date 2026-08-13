using NexaERP.Application.DTOs.Products;

namespace NexaERP.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
}
