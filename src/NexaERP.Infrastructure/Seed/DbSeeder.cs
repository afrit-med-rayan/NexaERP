using Microsoft.EntityFrameworkCore;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Enums;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Seed;

/// <summary>
/// Idempotent seeder — safe to call on every startup. Only inserts if data is missing.
/// Demo credentials are documented in README.md.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedRolesAsync(db);
        await SeedUsersAsync(db);
        await SeedCategoriesAsync(db);
        await SeedWarehousesAsync(db);
        await SeedProductsAsync(db);
        await SeedInventoryAsync(db);
        await SeedCustomersAsync(db);
        await SeedSalesOrdersAsync(db);
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        var roles = new[] { "Admin", "Manager", "SalesEmployee", "WarehouseEmployee", "Accountant" };
        foreach (var name in roles)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Name = name });
        }
        await db.SaveChangesAsync();
    }

    // ── Users (one per role) ──────────────────────────────────────────────────

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        var roleMap = await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);

        var users = new[]
        {
            ("Admin User",        "admin@nexaerp.com",     "Admin#2024",     "Admin"),
            ("Alice Manager",     "manager@nexaerp.com",   "Manager#2024",   "Manager"),
            ("Bob Sales",         "sales@nexaerp.com",     "Sales#2024",     "SalesEmployee"),
            ("Charlie Warehouse", "warehouse@nexaerp.com", "Warehouse#2024", "WarehouseEmployee"),
            ("Diana Accountant",  "accountant@nexaerp.com","Account#2024",   "Accountant"),
        };

        foreach (var (fullName, email, password, roleName) in users)
        {
            if (await db.Users.AnyAsync(u => u.Email == email)) continue;

            var user = new User
            {
                FullName     = fullName,
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleMap[roleName] });
            await db.SaveChangesAsync();
        }
    }

    // ── Categories ────────────────────────────────────────────────────────────

    private static async Task SeedCategoriesAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        db.Categories.AddRange(
            new Category { Name = "Electronics",    Description = "Computers, peripherals and gadgets" },
            new Category { Name = "Office Supplies", Description = "Stationery, paper and office consumables" },
            new Category { Name = "Furniture",       Description = "Desks, chairs and office furniture" },
            new Category { Name = "Networking",      Description = "Routers, switches and cables" },
            new Category { Name = "Software",        Description = "Licenses and subscriptions" }
        );
        await db.SaveChangesAsync();
    }

    // ── Warehouses ────────────────────────────────────────────────────────────

    private static async Task SeedWarehousesAsync(AppDbContext db)
    {
        if (await db.Warehouses.AnyAsync()) return;

        db.Warehouses.AddRange(
            new Warehouse { Name = "Main Warehouse",    Location = "Algiers, Algeria" },
            new Warehouse { Name = "Secondary Depot",   Location = "Oran, Algeria" }
        );
        await db.SaveChangesAsync();
    }

    // ── Products (~25 across 5 categories) ────────────────────────────────────

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var cats = await db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

        var products = new List<Product>
        {
            // Electronics (5)
            new() { Sku = "ELEC-001", Name = "Laptop Pro 15",        CategoryId = cats["Electronics"],    Price = 1200m, CostPrice = 850m,  ReorderThreshold = 5 },
            new() { Sku = "ELEC-002", Name = "Wireless Mouse",        CategoryId = cats["Electronics"],    Price = 25m,   CostPrice = 10m,   ReorderThreshold = 20 },
            new() { Sku = "ELEC-003", Name = "Mechanical Keyboard",   CategoryId = cats["Electronics"],    Price = 95m,   CostPrice = 55m,   ReorderThreshold = 10 },
            new() { Sku = "ELEC-004", Name = "27\" 4K Monitor",       CategoryId = cats["Electronics"],    Price = 450m,  CostPrice = 280m,  ReorderThreshold = 5 },
            new() { Sku = "ELEC-005", Name = "USB-C Hub 7-in-1",      CategoryId = cats["Electronics"],    Price = 40m,   CostPrice = 18m,   ReorderThreshold = 15 },

            // Office Supplies (5)
            new() { Sku = "OFFC-001", Name = "A4 Paper Ream",         CategoryId = cats["Office Supplies"], Price = 8m,    CostPrice = 4m,    ReorderThreshold = 50 },
            new() { Sku = "OFFC-002", Name = "Blue Ballpoint Pens x10",CategoryId = cats["Office Supplies"],Price = 5m,    CostPrice = 2m,    ReorderThreshold = 100 },
            new() { Sku = "OFFC-003", Name = "Stapler Heavy Duty",    CategoryId = cats["Office Supplies"], Price = 18m,   CostPrice = 8m,    ReorderThreshold = 20 },
            new() { Sku = "OFFC-004", Name = "File Folders x50",      CategoryId = cats["Office Supplies"], Price = 12m,   CostPrice = 5m,    ReorderThreshold = 30 },
            new() { Sku = "OFFC-005", Name = "Whiteboard A1",         CategoryId = cats["Office Supplies"], Price = 60m,   CostPrice = 30m,   ReorderThreshold = 8 },

            // Furniture (5)
            new() { Sku = "FURN-001", Name = "Ergonomic Chair",       CategoryId = cats["Furniture"],      Price = 320m,  CostPrice = 180m,  ReorderThreshold = 3 },
            new() { Sku = "FURN-002", Name = "Standing Desk 140cm",   CategoryId = cats["Furniture"],      Price = 550m,  CostPrice = 300m,  ReorderThreshold = 2 },
            new() { Sku = "FURN-003", Name = "3-Drawer Filing Cabinet",CategoryId = cats["Furniture"],     Price = 140m,  CostPrice = 75m,   ReorderThreshold = 4 },
            new() { Sku = "FURN-004", Name = "Bookshelf 5-Tier",      CategoryId = cats["Furniture"],      Price = 90m,   CostPrice = 45m,   ReorderThreshold = 5 },
            new() { Sku = "FURN-005", Name = "Conference Table 8-Seat",CategoryId = cats["Furniture"],     Price = 1200m, CostPrice = 700m,  ReorderThreshold = 1 },

            // Networking (5)
            new() { Sku = "NETW-001", Name = "Gigabit Router",        CategoryId = cats["Networking"],     Price = 85m,   CostPrice = 45m,   ReorderThreshold = 8 },
            new() { Sku = "NETW-002", Name = "24-Port Switch",        CategoryId = cats["Networking"],     Price = 220m,  CostPrice = 120m,  ReorderThreshold = 4 },
            new() { Sku = "NETW-003", Name = "CAT6 Cable 50m",        CategoryId = cats["Networking"],     Price = 25m,   CostPrice = 10m,   ReorderThreshold = 20 },
            new() { Sku = "NETW-004", Name = "Wireless AP AC1200",    CategoryId = cats["Networking"],     Price = 65m,   CostPrice = 35m,   ReorderThreshold = 10 },
            new() { Sku = "NETW-005", Name = "Network Rack 12U",      CategoryId = cats["Networking"],     Price = 180m,  CostPrice = 95m,   ReorderThreshold = 3 },

            // Software (5)
            new() { Sku = "SOFT-001", Name = "Antivirus 1-Year",      CategoryId = cats["Software"],       Price = 40m,   CostPrice = 20m,   ReorderThreshold = 10 },
            new() { Sku = "SOFT-002", Name = "Office Suite License",  CategoryId = cats["Software"],       Price = 150m,  CostPrice = 80m,   ReorderThreshold = 5 },
            new() { Sku = "SOFT-003", Name = "Design Tool Annual",    CategoryId = cats["Software"],       Price = 240m,  CostPrice = 130m,  ReorderThreshold = 3 },
            new() { Sku = "SOFT-004", Name = "Cloud Backup 1TB/yr",   CategoryId = cats["Software"],       Price = 80m,   CostPrice = 40m,   ReorderThreshold = 5 },
            new() { Sku = "SOFT-005", Name = "Password Manager Team", CategoryId = cats["Software"],       Price = 55m,   CostPrice = 25m,   ReorderThreshold = 5 },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    // ── Inventory (seed stock per warehouse) ───────────────────────────────────

    private static async Task SeedInventoryAsync(AppDbContext db)
    {
        if (await db.Inventories.AnyAsync()) return;

        var products   = await db.Products.ToListAsync();
        var warehouses = await db.Warehouses.ToListAsync();

        var rng = new Random(42);
        var entries = new List<Inventory>();

        foreach (var wh in warehouses)
        {
            foreach (var prod in products)
            {
                entries.Add(new Inventory
                {
                    ProductId   = prod.Id,
                    WarehouseId = wh.Id,
                    Quantity    = rng.Next(10, 100)
                });
            }
        }

        db.Inventories.AddRange(entries);
        await db.SaveChangesAsync();
    }

    // ── Customers ─────────────────────────────────────────────────────────────

    private static async Task SeedCustomersAsync(AppDbContext db)
    {
        if (await db.Customers.AnyAsync()) return;

        db.Customers.AddRange(
            new Customer { Name = "Société Générale Algérie", Email = "contact@sga.dz",    Phone = "+213 21 000 001", Address = "Algiers, Algeria" },
            new Customer { Name = "TechVision SARL",           Email = "info@techvision.dz", Phone = "+213 41 000 002", Address = "Oran, Algeria" },
            new Customer { Name = "Karim Benali",              Email = "karim.b@email.com",  Phone = "+213 55 111 222" },
            new Customer { Name = "Atlas Trade Co.",           Email = "trade@atlas.dz",     Phone = "+213 21 333 444", Address = "Constantine, Algeria" },
            new Customer { Name = "Innovate DZ",               Email = "hello@innovate.dz",  Phone = "+213 66 555 666", Address = "Annaba, Algeria" }
        );
        await db.SaveChangesAsync();
    }

    // ── Sales Orders (~15 historical) ─────────────────────────────────────────

    private static async Task SeedSalesOrdersAsync(AppDbContext db)
    {
        if (await db.SalesOrders.AnyAsync()) return;

        var adminUser  = await db.Users.FirstAsync(u => u.Email == "admin@nexaerp.com");
        var salesUser  = await db.Users.FirstAsync(u => u.Email == "sales@nexaerp.com");
        var customers  = await db.Customers.ToListAsync();
        var products   = await db.Products.ToListAsync();
        var warehouses = await db.Warehouses.ToListAsync();
        var mainWh     = warehouses.First();

        var rng = new Random(99);

        var orders = new List<(SalesOrder Order, List<(Product Prod, int Qty)> Items, bool Cancel)>
        {
            (MakeOrder(customers[0], adminUser, -60), new() { (products[0], 2), (products[3], 1) }, false),
            (MakeOrder(customers[1], salesUser, -55), new() { (products[5], 10),(products[6], 20) }, false),
            (MakeOrder(customers[2], salesUser, -50), new() { (products[10], 1),(products[11], 1) }, false),
            (MakeOrder(customers[3], adminUser, -45), new() { (products[15], 2),(products[16], 1) }, false),
            (MakeOrder(customers[4], salesUser, -40), new() { (products[20], 3),(products[21], 2) }, false),
            (MakeOrder(customers[0], adminUser, -35), new() { (products[1], 5), (products[2], 2) }, false),
            (MakeOrder(customers[1], salesUser, -30), new() { (products[7], 3), (products[8], 5) }, false),
            (MakeOrder(customers[2], adminUser, -25), new() { (products[12], 2),(products[13], 1) }, false),
            (MakeOrder(customers[3], salesUser, -20), new() { (products[17], 4),(products[18], 2) }, false),
            (MakeOrder(customers[4], adminUser, -18), new() { (products[22], 1),(products[23], 2) }, false),
            (MakeOrder(customers[0], salesUser, -15), new() { (products[4], 3), (products[9], 2) }, false),
            // Cancelled orders
            (MakeOrder(customers[1], adminUser, -12), new() { (products[14], 1),(products[19], 2) }, true),
            (MakeOrder(customers[2], salesUser, -10), new() { (products[0], 1)                  }, true),
            (MakeOrder(customers[3], adminUser, -8),  new() { (products[24], 5)                 }, true),
            (MakeOrder(customers[4], salesUser, -5),  new() { (products[3], 2), (products[16], 1)}, false),
        };

        foreach (var (order, items, cancel) in orders)
        {
            db.SalesOrders.Add(order);

            foreach (var (prod, qty) in items)
            {
                order.Items.Add(new SalesOrderItem
                {
                    ProductId = prod.Id,
                    Quantity  = qty,
                    UnitPrice = prod.Price
                });

                // Adjust inventory
                var inv = await db.Inventories.FirstOrDefaultAsync(
                    i => i.ProductId == prod.Id && i.WarehouseId == mainWh.Id);
                if (inv != null)
                    inv.Quantity = Math.Max(0, inv.Quantity - qty);

                db.StockMovements.Add(new Domain.Entities.StockMovement
                {
                    ProductId   = prod.Id,
                    WarehouseId = mainWh.Id,
                    MovementType = Domain.Enums.MovementType.Sale,
                    Quantity    = -qty,
                    ReferenceId = order.Id,
                    CreatedAt   = order.OrderDate,
                    CreatedById = order.CreatedById
                });
            }

            if (cancel)
            {
                order.Status = OrderStatus.Cancelled;
                // Restore stock
                foreach (var (prod, qty) in items)
                {
                    var inv = await db.Inventories.FirstOrDefaultAsync(
                        i => i.ProductId == prod.Id && i.WarehouseId == mainWh.Id);
                    if (inv != null) inv.Quantity += qty;

                    db.StockMovements.Add(new Domain.Entities.StockMovement
                    {
                        ProductId    = prod.Id,
                        WarehouseId  = mainWh.Id,
                        MovementType = Domain.Enums.MovementType.Adjustment,
                        Quantity     = qty,
                        ReferenceId  = order.Id,
                        CreatedAt    = order.OrderDate.AddMinutes(30),
                        CreatedById  = adminUser.Id
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static SalesOrder MakeOrder(Customer customer, User createdBy, int daysAgo) =>
        new()
        {
            CustomerId  = customer.Id,
            CreatedById = createdBy.Id,
            Status      = OrderStatus.Completed,
            OrderDate   = DateTime.UtcNow.AddDays(daysAgo)
        };
}
