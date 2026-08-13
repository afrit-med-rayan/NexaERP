-- ============================================================
-- sp_GetLowStockProducts
-- Returns products whose current Inventory.Quantity is at or
-- below their Product.ReorderThreshold.
-- @WarehouseId INT = NULL  → filter to one warehouse (optional)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GetLowStockProducts
    @WarehouseId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id             AS ProductId,
        p.Sku            AS Sku,
        p.Name           AS Name,
        c.Name           AS CategoryName,
        w.Id             AS WarehouseId,
        w.Name           AS WarehouseName,
        i.Quantity       AS Quantity,
        p.ReorderThreshold AS ReorderThreshold
    FROM Inventory i
    INNER JOIN Products   p ON p.Id = i.ProductId
    INNER JOIN Warehouses w ON w.Id = i.WarehouseId
    INNER JOIN Categories c ON c.Id = p.CategoryId
    WHERE
        i.Quantity <= p.ReorderThreshold
        AND p.IsActive = 1
        AND (@WarehouseId IS NULL OR i.WarehouseId = @WarehouseId)
    ORDER BY
        (p.ReorderThreshold - i.Quantity) DESC,   -- most critical first
        p.Name ASC;
END;
