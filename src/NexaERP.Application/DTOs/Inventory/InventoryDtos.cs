namespace NexaERP.Application.DTOs.Inventory;

public record InventoryResponse(
    int Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int WarehouseId,
    string WarehouseName,
    int Quantity,
    int ReorderThreshold,
    bool IsLowStock);

public record AdjustInventoryRequest(
    Guid ProductId,
    int WarehouseId,
    int QuantityDelta,
    string? Note);

public record LowStockProductResponse(
    Guid ProductId,
    string Sku,
    string Name,
    string CategoryName,
    int WarehouseId,
    string WarehouseName,
    int Quantity,
    int ReorderThreshold);
