<div align="center">
  <h1>🚀 NexaERP</h1>
  <p><strong>A Modern, Full-Stack Enterprise Resource Planning Application</strong></p>
  
  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
  [![React](https://img.shields.io/badge/React-18-61DAFB?style=for-the-badge&logo=react&logoColor=black)](https://reactjs.org/)
  [![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
  [![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
  [![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)
</div>

<br />

NexaERP is a comprehensive, production-ready Enterprise Resource Planning (ERP) web application. It streamlines product management, inventory tracking, customer relationships, and sales order processing through a clean, role-based interface.

---

## 📋 Table of Contents
- [✨ Features](#-features)
- [🏗️ Architecture & Database Design](#️-architecture--database-design)
- [💻 Tech Stack](#-tech-stack)
- [🚀 Getting Started](#-getting-started)
- [🔑 Demo Credentials](#-demo-credentials)
- [📖 API Documentation](#-api-documentation)
- [🧪 Testing](#-testing)
- [🗺️ Roadmap](#️-roadmap)
- [📄 License](#-license)

---

## ✨ Features

- **🔐 Authentication & Authorization:** Role-based access control (Admin, Manager, WarehouseEmployee, SalesEmployee, Accountant) secured via JWT.
- **📦 Product & Category Management:** Comprehensive CRUD operations for maintaining detailed product catalogs and categorizations.
- **🏢 Inventory Management:** Real-time stock tracking across multiple warehouses, automated low-stock alerts, and manual adjustment auditing.
- **👥 Customer Management:** Centralized registry of customer profiles, contact information, and shipping addresses.
- **🛒 Sales Orders:** Streamlined order creation featuring real-time stock validation, automated inventory deduction, and robust cancellation workflows.
- **📊 Dashboard & Analytics:** Live tracking of Key Performance Indicators (KPIs) and a dynamic 30-day sales revenue visualization.

## 🏗️ Architecture & Database Design

NexaERP follows a clean, multi-layered architecture separating presentation, application logic, domain models, and data access infrastructure.

<details>
<summary><b>View High-Level Architecture</b></summary>
<br>

* Backend: Multi-tier .NET 8 API (Controllers -> Application Services -> Domain -> Infrastructure).
* Frontend: React Single Page Application communicating via REST.
* Database: SQL Server 2022 accessed via Entity Framework Core.

</details>

👉 **For detailed Mermaid diagrams, see the [Architecture Documentation](docs/architecture.md).**

## 💻 Tech Stack

### Backend
* **Framework:** C# / ASP.NET Core 8 Web API
* **ORM:** Entity Framework Core
* **Security:** JWT Bearer Authentication & BCrypt Password Hashing
* **Testing:** xUnit & Moq

### Frontend
* **Core:** React 18, TypeScript, Vite
* **State Management:** Zustand
* **Networking:** Axios (with custom interceptors)
* **UI/UX:** Vanilla CSS (Glassmorphism design system), Lucide React, Recharts

### Infrastructure
* **Containerization:** Docker & Docker Compose
* **Web Server:** Nginx (Alpine)
* **Database:** Microsoft SQL Server 2022

---

## 🚀 Getting Started

You can run the entire NexaERP application stack locally with a single command using Docker Compose.

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.
- Git.

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/afrit-med-rayan/NexaERP.git
   cd NexaERP
   ```

2. **Start the application:**
   ```bash
   docker compose up --build
   ```
   > **Note:** The backend API container is configured to wait for the SQL Server container to become healthy. Upon successful connection, the API will automatically run EF Core migrations and seed the database with initial users and roles.

3. **Access the application:**
   - 🌐 **Frontend UI:** [http://localhost:3000](http://localhost:3000)
   - ⚙️ **Backend API:** [http://localhost:5000/api](http://localhost:5000/api)
   - 📚 **Swagger Docs:** [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🔑 Demo Credentials

The database is automatically seeded with default users covering all system roles. You can log into the frontend using any of the accounts below. 

> **Password for all demo accounts:** `Role#2024`

| Role | Email | Capabilities |
|------|-------|-------------|
| **Admin** | `admin@nexaerp.com` | Full system access. |
| **Manager** | `manager@nexaerp.com` | Can manage products, categories, view inventory, and cancel sales orders. |
| **Warehouse** | `warehouse@nexaerp.com` | Can view inventory and perform manual stock adjustments. |
| **Sales** | `sales@nexaerp.com` | Can create customers and place sales orders. |
| **Accountant** | `accountant@nexaerp.com` | Can view customers and sales orders. |

---

## 📖 API Documentation

The REST API is fully documented using Swagger/OpenAPI. Once the application is running via Docker Compose, navigate to [http://localhost:5000/swagger](http://localhost:5000/swagger) to view the interactive API documentation and test endpoints directly from your browser.

---

## 🧪 Testing

The backend includes a comprehensive suite of unit and integration tests covering domain logic, application services, and concurrent transaction handling.

To run the tests locally (requires the .NET 8 SDK):
```bash
cd tests/NexaERP.Tests
dotnet test
```

---

## 🗺️ Roadmap

Future enhancements planned for NexaERP:
- [ ] **Suppliers & Purchase Orders:** Manage vendor relationships and track inbound inventory.
- [ ] **Payments & Invoicing:** Generate PDF invoices from sales orders and track payment lifecycles.
- [ ] **Warehouse Transfers:** Move stock seamlessly between different physical warehouse locations.
- [ ] **Advanced Reporting:** Export reports to PDF/Excel and add custom date-range filters to the dashboard.

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
