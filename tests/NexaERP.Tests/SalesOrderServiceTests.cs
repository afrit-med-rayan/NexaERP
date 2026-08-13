using FluentAssertions;
using NexaERP.Application.DTOs.SalesOrders;
using NexaERP.Domain.Enums;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Services;
using NexaERP.Tests.Helpers;

namespace NexaERP.Tests;

public class SalesOrderServiceTests
{
    // ── Helper ────────────────────────────────────────────────────────────────

    private static (SalesOrderService svc, Guid userId, Guid productId,
                    Guid inactiveProductId, Guid customerId)
        BuildSut()
    {
        var db    = TestDbFactory.Create();
        var seeds = TestDbFactory.SeedMinimal(db);
        var svc   = new SalesOrderService(db);
        var customerId = db.Customers.First().Id;

        return (svc, seeds.adminUser.Id, seeds.activeProduct.Id,
                seeds.inactiveProduct.Id, customerId);
    }

    // ── Test 1: Sufficient stock → order succeeds, inventory decrements ───────

    [Fact]
    public async Task CreateOrder_WithSufficientStock_Succeeds_And_DecrementsInventory()
    {
        var (svc, userId, productId, _, customerId) = BuildSut();
        var db = TestDbFactory.Create();
        // Use same db — rebuild svc with it
        var db2    = TestDbFactory.Create("suf_stock");
        var seeds2 = TestDbFactory.SeedMinimal(db2);
        var svc2   = new SalesOrderService(db2);

        var request = new CreateSalesOrderRequest(
            seeds2.adminUser.Id == Guid.Empty ? seeds2.adminUser.Id : db2.Customers.First().Id,
            WarehouseId: 1,
            Items: new[] { new SalesOrderItemRequest(seeds2.activeProduct.Id, 10) });

        var result = await svc2.CreateAsync(request, seeds2.adminUser.Id);

        result.Should().NotBeNull();
        result.Status.Should().Be("Completed");
        result.Items.Should().HaveCount(1);
        result.OrderTotal.Should().Be(10 * 100m); // qty * price

        var inventory = db2.Inventories.First(i => i.ProductId == seeds2.activeProduct.Id);
        inventory.Quantity.Should().Be(40); // 50 - 10

        var movements = db2.StockMovements.Where(sm => sm.ProductId == seeds2.activeProduct.Id).ToList();
        movements.Should().HaveCount(1);
        movements[0].MovementType.Should().Be(MovementType.Sale);
        movements[0].Quantity.Should().Be(-10);
    }

    // ── Test 2: Insufficient stock → BusinessException, no DB changes ─────────

    [Fact]
    public async Task CreateOrder_WithInsufficientStock_ThrowsBusinessException_NoDbChanges()
    {
        var db     = TestDbFactory.Create("insuf_stock");
        var seeds  = TestDbFactory.SeedMinimal(db);
        var svc    = new SalesOrderService(db);
        var custId = db.Customers.First().Id;

        var request = new CreateSalesOrderRequest(
            custId, WarehouseId: 1,
            Items: new[] { new SalesOrderItemRequest(seeds.activeProduct.Id, 999) }); // way over 50

        var act = async () => await svc.CreateAsync(request, seeds.adminUser.Id);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Insufficient stock*");

        db.SalesOrders.Should().BeEmpty();
        db.Inventories.First(i => i.ProductId == seeds.activeProduct.Id).Quantity.Should().Be(50);
    }

    // ── Test 3: Cancel completed order → inventory restored, compensating SM ──

    [Fact]
    public async Task CancelOrder_RestoresInventory_And_WritesCompensatingStockMovement()
    {
        var db    = TestDbFactory.Create("cancel_order");
        var seeds = TestDbFactory.SeedMinimal(db);
        var svc   = new SalesOrderService(db);
        var custId = db.Customers.First().Id;

        // First create the order
        var createReq = new CreateSalesOrderRequest(
            custId, WarehouseId: 1,
            Items: new[] { new SalesOrderItemRequest(seeds.activeProduct.Id, 5) });
        var created = await svc.CreateAsync(createReq, seeds.adminUser.Id);

        db.Inventories.First(i => i.ProductId == seeds.activeProduct.Id).Quantity.Should().Be(45);

        // Now cancel it
        var cancelled = await svc.CancelAsync(created.Id, seeds.adminUser.Id);

        cancelled.Status.Should().Be("Cancelled");

        // Inventory restored
        db.Inventories.First(i => i.ProductId == seeds.activeProduct.Id).Quantity.Should().Be(50);

        // Compensating movement
        var movements = db.StockMovements
            .Where(sm => sm.ProductId == seeds.activeProduct.Id)
            .ToList();

        movements.Should().HaveCount(2); // Sale + Adjustment
        movements.Should().Contain(sm => sm.MovementType == MovementType.Adjustment && sm.Quantity == 5);
    }

    // ── Test 4: Inactive product → BusinessException ──────────────────────────

    [Fact]
    public async Task CreateOrder_WithInactiveProduct_ThrowsBusinessException()
    {
        var db     = TestDbFactory.Create("inactive_prod");
        var seeds  = TestDbFactory.SeedMinimal(db);
        var svc    = new SalesOrderService(db);
        var custId = db.Customers.First().Id;

        var request = new CreateSalesOrderRequest(
            custId, WarehouseId: 1,
            Items: new[] { new SalesOrderItemRequest(seeds.inactiveProduct.Id, 1) });

        var act = async () => await svc.CreateAsync(request, seeds.adminUser.Id);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*inactive*");

        db.SalesOrders.Should().BeEmpty();
    }
}
