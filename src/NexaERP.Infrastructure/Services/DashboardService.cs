using Microsoft.EntityFrameworkCore;
using NexaERP.Application.DTOs.Dashboard;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Enums;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var revenueThisMonth = await _db.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= monthStart)
            .SelectMany(o => o.Items)
            .SumAsync(i => (decimal?)( i.Quantity * i.UnitPrice)) ?? 0m;

        var ordersThisMonth = await _db.SalesOrders
            .CountAsync(o => o.Status == OrderStatus.Completed && o.OrderDate >= monthStart);

        var lowStockCount = await _db.Inventories
            .Where(i => i.Quantity <= i.Product.ReorderThreshold && i.Product.IsActive)
            .CountAsync();

        var totalActiveProducts = await _db.Products.CountAsync(p => p.IsActive);
        var totalCustomers      = await _db.Customers.CountAsync();

        return new DashboardSummaryResponse(
            revenueThisMonth,
            ordersThisMonth,
            lowStockCount,
            totalActiveProducts,
            totalCustomers);
    }

    public async Task<IEnumerable<SalesOverviewPoint>> GetSalesOverviewAsync(int months)
    {
        months = Math.Clamp(months, 1, 24);
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var raw = await _db.SalesOrders
            .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= cutoff)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue    = g.SelectMany(o => o.Items).Sum(i => i.Quantity * i.UnitPrice),
                OrderCount = g.Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return raw.Select(g => new SalesOverviewPoint(
            $"{g.Year}-{g.Month:D2}",
            g.Revenue,
            g.OrderCount));
    }

    /// <summary>
    /// Uses vw_ProductSalesSummary view (created separately in /database/Views).
    /// Falls back to LINQ query when the view doesn't yet exist in the DB.
    /// </summary>
    public async Task<IEnumerable<ProductSalesSummaryResponse>> GetProductSalesSummaryAsync()
    {
        // Query via vw_ProductSalesSummary
        var results = await _db.Database
            .SqlQuery<ProductSalesSummaryRaw>(
                $"SELECT ProductId, ProductName, Sku, UnitsSold, TotalRevenue FROM vw_ProductSalesSummary")
            .ToListAsync();

        return results.Select(r => new ProductSalesSummaryResponse(
            r.ProductId, r.ProductName, r.Sku, r.UnitsSold, r.TotalRevenue));
    }

    private sealed class ProductSalesSummaryRaw
    {
        public Guid ProductId      { get; set; }
        public string ProductName  { get; set; } = string.Empty;
        public string Sku          { get; set; } = string.Empty;
        public int UnitsSold       { get; set; }
        public decimal TotalRevenue{ get; set; }
    }
}
