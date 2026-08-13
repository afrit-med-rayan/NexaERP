using NexaERP.Application.DTOs.Products;

namespace NexaERP.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);
    Task DeleteAsync(Guid id);
}
