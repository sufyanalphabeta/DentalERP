using DentalERP.Modules.Inventory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Inventory;

public sealed class StockBatchTests
{
    private static readonly Guid ItemId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    [Fact]
    public void Create_ValidBatch_SetsProperties()
    {
        var receivedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var batch = StockBatch.Create(ItemId, WarehouseId, 100m, 5.00m, receivedDate);

        batch.ItemId.Should().Be(ItemId);
        batch.WarehouseId.Should().Be(WarehouseId);
        batch.Quantity.Should().Be(100m);
        batch.UnitCost.Should().Be(5.00m);
        batch.IsDepleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ZeroQuantity_Throws()
    {
        var act = () => StockBatch.Create(ItemId, WarehouseId, 0m, 5m, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NegativeUnitCost_Throws()
    {
        var act = () => StockBatch.Create(ItemId, WarehouseId, 10m, -1m, DateOnly.FromDateTime(DateTime.UtcNow));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deduct_ReducesQuantity()
    {
        var batch = StockBatch.Create(ItemId, WarehouseId, 100m, 5m, DateOnly.FromDateTime(DateTime.UtcNow));
        batch.Deduct(30m);
        batch.Quantity.Should().Be(70m);
        batch.IsDepleted.Should().BeFalse();
    }

    [Fact]
    public void Deduct_FullQuantity_SetsIsDepleted()
    {
        var batch = StockBatch.Create(ItemId, WarehouseId, 50m, 5m, DateOnly.FromDateTime(DateTime.UtcNow));
        batch.Deduct(50m);
        batch.Quantity.Should().Be(0m);
        batch.IsDepleted.Should().BeTrue();
    }

    [Fact]
    public void Deduct_MoreThanQuantity_SetsQuantityToZeroAndDepleted()
    {
        var batch = StockBatch.Create(ItemId, WarehouseId, 10m, 5m, DateOnly.FromDateTime(DateTime.UtcNow));
        batch.Deduct(15m);
        batch.Quantity.Should().Be(0m);
        batch.IsDepleted.Should().BeTrue();
    }

    [Fact]
    public void AddQuantity_IncreasesQuantityAndClearsDepleted()
    {
        var batch = StockBatch.Create(ItemId, WarehouseId, 10m, 5m, DateOnly.FromDateTime(DateTime.UtcNow));
        batch.Deduct(10m);
        batch.IsDepleted.Should().BeTrue();
        batch.AddQuantity(5m);
        batch.Quantity.Should().Be(5m);
        batch.IsDepleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithBatchNumberAndExpiry_SetsProperties()
    {
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6));
        var batch = StockBatch.Create(ItemId, WarehouseId, 20m, 3m,
            DateOnly.FromDateTime(DateTime.UtcNow), "BATCH-001", expiry);

        batch.BatchNumber.Should().Be("BATCH-001");
        batch.ExpiryDate.Should().Be(expiry);
    }

    [Fact]
    public void IsExpiringSoon_WhenWithinWindow_ReturnsTrue()
    {
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var batch = StockBatch.Create(ItemId, WarehouseId, 10m, 2m,
            DateOnly.FromDateTime(DateTime.UtcNow), expiryDate: expiry);

        batch.IsExpiringSoon(60).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenPastExpiry_ReturnsTrue()
    {
        var expiry = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var batch = StockBatch.Create(ItemId, WarehouseId, 10m, 2m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)), expiryDate: expiry);

        batch.IsExpired.Should().BeTrue();
    }
}
