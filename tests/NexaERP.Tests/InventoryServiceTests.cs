using FluentAssertions;
using NexaERP.Application.DTOs.Inventory;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Services;
using NexaERP.Tests.Helpers;

namespace NexaERP.Tests;

public class InventoryServiceTests
{
    // ── Test: Adjustment below zero is rejected ────────────────────────────────

    [Fact]
    public async Task AdjustInventory_BelowZero_ThrowsBusinessException()
    {
        var db    = TestDbFactory.Create("inv_below_zero");
        var seeds = TestDbFactory.SeedMinimal(db);
        var svc   = new InventoryService(db);

        // Current quantity = 50; try to remove 60
        var request = new AdjustInventoryRequest(
            seeds.activeProduct.Id,
            WarehouseId: 1,
            QuantityDelta: -60,
            Note: "Test over-adjustment");

        var act = async () => await svc.AdjustAsync(request, seeds.adminUser.Id);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*negative*");

        // Quantity must remain unchanged
        db.Inventories.First(i => i.ProductId == seeds.activeProduct.Id)
            .Quantity.Should().Be(50);
    }

    // ── Bonus: Valid positive adjustment succeeds ─────────────────────────────

    [Fact]
    public async Task AdjustInventory_PositiveDelta_IncreasesQuantity()
    {
        var db    = TestDbFactory.Create("inv_positive");
        var seeds = TestDbFactory.SeedMinimal(db);
        var svc   = new InventoryService(db);

        var request = new AdjustInventoryRequest(
            seeds.activeProduct.Id, WarehouseId: 1, QuantityDelta: 20, Note: null);

        var result = await svc.AdjustAsync(request, seeds.adminUser.Id);

        result.Quantity.Should().Be(70); // 50 + 20
        db.StockMovements.Should().ContainSingle();
    }
}
