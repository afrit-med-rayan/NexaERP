using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaERP.Application.Interfaces;

namespace NexaERP.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>KPI summary: revenue this month, order count, low-stock count, totals.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
        => Ok(await _dashboard.GetSummaryAsync());

    /// <summary>Monthly revenue + order count time series. ?months=6 (default 6, max 24).</summary>
    [HttpGet("sales-overview")]
    public async Task<IActionResult> SalesOverview([FromQuery] int months = 6)
        => Ok(await _dashboard.GetSalesOverviewAsync(months));

    /// <summary>Product sales summary via vw_ProductSalesSummary (revenue desc).</summary>
    [HttpGet("product-sales")]
    public async Task<IActionResult> ProductSales()
        => Ok(await _dashboard.GetProductSalesSummaryAsync());
}
