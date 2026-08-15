# NexaERP Architecture

NexaERP is built using a modern, multi-tier architecture to ensure scalability, maintainability, and clean separation of concerns.

## System Architecture

```mermaid
graph TD
    Client[React SPA Client]
    Client -->|REST API / JSON| API[NexaERP.API]
    
    subgraph Backend
        API --> App[NexaERP.Application]
        App --> Domain[NexaERP.Domain]
        API --> Infra[NexaERP.Infrastructure]
        App -.-> Infra
    end
    
    Infra -->|EF Core / ADO.NET| DB[(SQL Server 2022)]
```

### Layers Description
- **Client (Frontend)**: Vite + React SPA built with TypeScript. Uses `axios` for API calls and `zustand` for state management.
- **NexaERP.API**: The presentation layer containing ASP.NET Core Controllers, JWT authentication, and Swagger documentation.
- **NexaERP.Application**: The application logic layer. Contains Data Transfer Objects (DTOs), business service interfaces, and orchestration logic.
- **NexaERP.Domain**: The core domain model. Contains Entities, Enums, and domain exceptions (e.g., `BusinessException`).
- **NexaERP.Infrastructure**: The data access layer. Implements Application interfaces using Entity Framework Core, handles DbContext, migrations, and direct ADO.NET stored procedure calls.

## Database Schema

```mermaid
erDiagram
    Users {
        string Id PK
        string FullName
        string Email
        string PasswordHash
        string Role
    }
    
    Categories {
        string Id PK
        string Name
        string Description
    }
    
    Products {
        string Id PK
        string SKU
        string Name
        string CategoryId FK
        decimal Price
        decimal CostPrice
        int ReorderThreshold
        bool IsActive
    }
    
    Warehouses {
        string Id PK
        string Name
        string Location
    }
    
    Inventory {
        string ProductId PK,FK
        string WarehouseId PK,FK
        int Quantity
    }
    
    StockMovements {
        string Id PK
        string ProductId FK
        string WarehouseId FK
        string UserId FK
        int QuantityChange
        string Reason
        datetime Date
    }
    
    Customers {
        string Id PK
        string Name
        string Email
        string Phone
        string Address
    }
    
    SalesOrders {
        string Id PK
        string CustomerId FK
        string CreatedById FK
        datetime OrderDate
        string Status
        decimal TotalAmount
    }
    
    SalesOrderItems {
        string Id PK
        string SalesOrderId FK
        string ProductId FK
        int Quantity
        decimal UnitPrice
    }
    
    Categories ||--o{ Products : contains
    Products ||--o{ Inventory : has
    Warehouses ||--o{ Inventory : holds
    Products ||--o{ StockMovements : logs
    Users ||--o{ StockMovements : records
    Users ||--o{ SalesOrders : creates
    Customers ||--o{ SalesOrders : places
    SalesOrders ||--|{ SalesOrderItems : includes
    Products ||--o{ SalesOrderItems : contains
```
