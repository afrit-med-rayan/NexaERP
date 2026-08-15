# NexaERP

NexaERP is a modern, full-stack Enterprise Resource Planning (ERP) web application built to streamline product management, inventory tracking, customer relationships, and sales order processing. It features a robust multi-tiered backend built with .NET 8 and a sleek, responsive React frontend.

## Features

- **Authentication & Authorization**: Role-based access control (Admin, Manager, WarehouseEmployee, SalesEmployee, Accountant) with JWT authentication.
- **Product & Category Management**: Comprehensive CRUD operations for managing product catalogs and categorization.
- **Inventory Management**: Real-time stock tracking across multiple warehouses, low-stock alerts, and manual stock adjustments.
- **Customer Management**: Maintain a central registry of customer profiles, contact information, and addresses.
- **Sales Orders**: Streamlined order creation with real-time stock validation, automated inventory deduction, and order cancellation workflows.
- **Dashboard & Analytics**: Live tracking of key performance indicators (KPIs) and a 30-day sales revenue chart.

## Architecture & Database Design

NexaERP follows a clean, multi-layered architecture separating presentation, application logic, domain models, and data access infrastructure.

Please see the [Architecture Documentation](docs/architecture.md) for detailed Mermaid diagrams of the system architecture and database schema.

## Tech Stack

**Backend:**
- C# / .NET 8 (ASP.NET Core Web API)
- Entity Framework Core (SQL Server)
- JWT Bearer Authentication
- xUnit & Moq (Testing)

**Frontend:**
- React (via Vite)
- TypeScript
- Zustand (State Management)
- Axios (API Client)
- Recharts (Data Visualization)
- Lucide React (Icons)
- Vanilla CSS with CSS Variables (Premium Glassmorphism UI)

**Infrastructure:**
- Docker & Docker Compose
- Nginx (Frontend Hosting)
- SQL Server 2022 (Database)

## Getting Started

You can run the entire NexaERP application stack, including the database, backend API, and frontend client, using Docker Compose.

1. **Clone the repository:**
   ```bash
   git clone https://github.com/afrit-med-rayan/NexaERP.git
   cd NexaERP
   ```

2. **Start the application:**
   ```bash
   docker compose up --build
   ```

   *Note: The API service waits for the SQL Server container to become healthy before starting. Upon startup, the API will automatically apply EF Core database migrations and seed the initial data.*

3. **Access the application:**
   - **Frontend:** http://localhost:3000
   - **Backend API:** http://localhost:5000/api
   - **Swagger UI:** http://localhost:5000/swagger

### Demo Credentials

The database is seeded with a set of default users covering various roles. Use the following credentials to log in (Password for all users is `Role#2024`):

| Role | Email |
|------|-------|
| Admin | admin@nexaerp.com |
| Manager | manager@nexaerp.com |
| Warehouse Employee | warehouse@nexaerp.com |
| Sales Employee | sales@nexaerp.com |
| Accountant | accountant@nexaerp.com |

## API Documentation

The REST API is fully documented using Swagger/OpenAPI. Once the application is running, navigate to `http://localhost:5000/swagger` to view the interactive API documentation and test endpoints.

## Testing

The backend includes a comprehensive suite of unit and integration tests using xUnit, testing the core domain logic, application services, and transaction handling.

To run the tests locally:
```bash
cd tests/NexaERP.Tests
dotnet test
```

## Roadmap

Future planned features for NexaERP include:
- **Suppliers & Purchase Orders**: Managing vendor relationships and tracking inbound inventory.
- **Payments & Invoicing**: Generating invoices from sales orders and tracking payment statuses.
- **Warehouse Transfers**: Moving stock seamlessly between different warehouse locations.
- **Advanced Reporting**: Exporting reports to PDF/Excel and adding custom date-range filters to the dashboard.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
