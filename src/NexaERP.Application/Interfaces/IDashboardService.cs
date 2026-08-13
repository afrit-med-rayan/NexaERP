using NexaERP.Application.DTOs.Dashboard;

namespace NexaERP.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
    Task<IEnumerable<SalesOverviewPoint>> GetSalesOverviewAsync(int months);
    Task<IEnumerable<ProductSalesSummaryResponse>> GetProductSalesSummaryAsync();
}
