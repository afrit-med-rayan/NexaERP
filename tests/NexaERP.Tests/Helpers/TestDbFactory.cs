using Microsoft.EntityFrameworkCore;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Enums;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Tests.Helpers;

/// <summary>
/// Creates a fresh EF Core InMemory DbContext pre-seeded with
/// minimal data needed by the unit tests.
/// Each test gets its own database name to avoid state leakage.
/// </summary>
public static class TestDbFactory
{
    public static AppDbContext Create(string dbName = "")
    {
        if (string.IsNullOrEmpty(dbName))
            dbName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            // Suppress transaction warning: InMemory silently ignores transactions,
            // which is acceptable for unit tests that don't rely on rollback.
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    // ── Seed helpers ──────────────────────────────────────────────────────────

    public static (User adminUser, Role adminRole, Category cat,
                   Warehouse wh, Product activeProduct, Product inactiveProduct)
        SeedMinimal(AppDbContext db)
    {
        var adminRole = new Role { Id = 1, Name = "Admin" };
        db.Roles.Add(adminRole);

        var adminUser = new User
        {
            FullName     = "Test Admin",
            Email        = "admin@test.com",
            PasswordHash = "hashed"
        };
        db.Users.Add(adminUser);

        var cat = new Category { Id = 1, Name = "Electronics", Description = "Test" };
        db.Categories.Add(cat);

        var wh = new Warehouse { Id = 1, Name = "Main Warehouse" };
        db.Warehouses.Add(wh);

        var activeProduct = new Product
        {
            Sku              = "PROD-001",
            Name             = "Widget",
            CategoryId       = 1,
            Price            = 100m,
            CostPrice        = 50m,
            ReorderThreshold = 5,
            IsActive         = true
        };
        var inactiveProduct = new Product
        {
            Sku              = "PROD-INACTIVE",
            Name             = "Discontinued Widget",
            CategoryId       = 1,
            Price            = 50m,
            CostPrice        = 20m,
            ReorderThreshold = 5,
            IsActive         = false
        };
        db.Products.AddRange(activeProduct, inactiveProduct);

        var customer = new Customer { Name = "Test Corp", Email = "test@corp.com" };
        db.Customers.Add(customer);

        db.SaveChanges();

        // Stock: 50 units of active product, 10 of inactive
        db.Inventories.AddRange(
            new Inventory { ProductId = activeProduct.Id,   WarehouseId = 1, Quantity = 50 },
            new Inventory { ProductId = inactiveProduct.Id, WarehouseId = 1, Quantity = 10 }
        );
        db.SaveChanges();

        return (adminUser, adminRole, cat, wh, activeProduct, inactiveProduct);
    }
}
