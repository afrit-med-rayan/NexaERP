-- ============================================================
-- sp_CompleteSalesOrder
-- DB-level safety net that mirrors the EF Core transaction path.
-- Exists for defense-in-depth and demonstrates raw T-SQL skills.
-- Primary write path is through EF Core in SalesOrderService.
-- ============================================================
CREATE OR ALTER PROCEDURE sp_CompleteSalesOrder
    @OrderId   UNIQUEIDENTIFIER,
    @UserId    UNIQUEIDENTIFIER,
    @WarehouseId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- auto-rollback on any error

    BEGIN TRANSACTION;

    -- Guard: order must exist and be in Pending status
    IF NOT EXISTS (
        SELECT 1 FROM SalesOrders
        WHERE Id = @OrderId AND Status = 0   -- 0 = Pending
    )
    BEGIN
        ROLLBACK;
        RAISERROR('Order not found or not in Pending status.', 16, 1);
        RETURN;
    END

    -- Validate stock for every line item (fail fast before any writes)
    IF EXISTS (
        SELECT 1
        FROM SalesOrderItems soi
        JOIN Inventory inv
          ON inv.ProductId = soi.ProductId AND inv.WarehouseId = @WarehouseId
        WHERE soi.SalesOrderId = @OrderId
          AND inv.Quantity < soi.Quantity
    )
    BEGIN
        ROLLBACK;
        RAISERROR('Insufficient stock for one or more order items.', 16, 1);
        RETURN;
    END

    -- Decrement inventory for each line item
    UPDATE inv
    SET    inv.Quantity = inv.Quantity - soi.Quantity
    FROM   Inventory inv
    JOIN   SalesOrderItems soi
      ON   soi.ProductId = soi.ProductId
     AND   inv.WarehouseId = @WarehouseId
    WHERE  soi.SalesOrderId = @OrderId;

    -- Insert StockMovement rows (Sale, negative quantity)
    INSERT INTO StockMovements (Id, ProductId, WarehouseId, MovementType, Quantity,
                                ReferenceId, CreatedAt, CreatedById)
    SELECT
        NEWID(),
        soi.ProductId,
        @WarehouseId,
        1,              -- 1 = Sale
        -soi.Quantity,
        @OrderId,
        GETUTCDATE(),
        @UserId
    FROM SalesOrderItems soi
    WHERE soi.SalesOrderId = @OrderId;

    -- Mark order as Completed
    UPDATE SalesOrders
    SET    Status = 1   -- 1 = Completed
    WHERE  Id = @OrderId;

    COMMIT;
END;
