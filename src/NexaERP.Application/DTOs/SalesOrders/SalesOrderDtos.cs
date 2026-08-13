namespace NexaERP.Application.DTOs.SalesOrders;

public record SalesOrderItemRequest(Guid ProductId, int Quantity);

public record CreateSalesOrderRequest(
    Guid CustomerId,
    int WarehouseId,
    IEnumerable<SalesOrderItemRequest> Items);

public record SalesOrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record SalesOrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    string CreatedBy,
    IEnumerable<SalesOrderItemResponse> Items,
    decimal OrderTotal);
