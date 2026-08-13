-- ============================================================
-- vw_ProductSalesSummary
-- Units sold + revenue per product across all Completed orders,
-- ordered by revenue descending.
-- ============================================================
CREATE OR ALTER VIEW vw_ProductSalesSummary AS
SELECT
    p.Id                                        AS ProductId,
    p.Name                                      AS ProductName,
    p.Sku                                       AS Sku,
    SUM(soi.Quantity)                           AS UnitsSold,
    SUM(soi.Quantity * soi.UnitPrice)           AS TotalRevenue
FROM SalesOrderItems soi
INNER JOIN Products     p  ON p.Id  = soi.ProductId
INNER JOIN SalesOrders  so ON so.Id = soi.SalesOrderId
WHERE
    so.Status = 1   -- 1 = Completed
GROUP BY
    p.Id, p.Name, p.Sku
-- Note: ORDER BY is NOT allowed in a SQL Server view;
-- the caller (DashboardService) orders by TotalRevenue DESC.
;
