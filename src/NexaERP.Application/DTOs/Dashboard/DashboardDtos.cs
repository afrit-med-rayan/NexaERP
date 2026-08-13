namespace NexaERP.Application.DTOs.Dashboard;

public record DashboardSummaryResponse(
    decimal TotalRevenueThisMonth,
    int OrdersThisMonth,
    int LowStockCount,
    int TotalActiveProducts,
    int TotalCustomers);

public record SalesOverviewPoint(
    string Period,   // "YYYY-MM" or "YYYY-MM-DD"
    decimal Revenue,
    int OrderCount);

public record ProductSalesSummaryResponse(
    Guid ProductId,
    string ProductName,
    string Sku,
    int UnitsSold,
    decimal TotalRevenue);
