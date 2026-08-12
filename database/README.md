# NexaERP — Database Artifacts

Raw SQL files that live alongside the EF Core migrations to demonstrate deliberate SQL Server usage.

## Structure

```
database/
├── Views/
│   └── vw_ProductSalesSummary.sql     # Added in Phase 7
├── StoredProcedures/
│   ├── sp_GetLowStockProducts.sql     # Added in Phase 4
│   └── sp_CompleteSalesOrder.sql      # Added in Phase 6
└── Seed/
    └── seed-data.sql                  # Reference only (app uses C# seeder)
```

## Index Notes (Query Plan Analysis)

_To be populated in Phase 4 once indexes are added._

### `SalesOrderItems(ProductId)`

**Query**: Sales summary joining `SalesOrderItems` to `Products` for revenue calculation.

| | Before Index | After Index |
|---|---|---|
| Scan type | Table Scan | Index Seek |
| Estimated rows | All rows | Filtered |

### `StockMovements(ProductId, WarehouseId, CreatedAt)`

**Query**: Movement history filtered by product and warehouse, ordered by date.

| | Before Index | After Index |
|---|---|---|
| Scan type | Table Scan | Index Seek |
| Sort needed | Yes | No (covered) |
