using NexaERP.Application.DTOs.Inventory;

namespace NexaERP.Application.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryResponse>> GetAllAsync();
    Task<IEnumerable<LowStockProductResponse>> GetLowStockAsync(int? warehouseId);
    Task<InventoryResponse> AdjustAsync(AdjustInventoryRequest request, Guid performedByUserId);
}
