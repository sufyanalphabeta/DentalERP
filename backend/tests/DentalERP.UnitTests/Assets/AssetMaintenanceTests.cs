using DentalERP.Modules.Assets.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Assets;

public class AssetMaintenanceTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var assetId = Guid.NewGuid();
        var m = AssetMaintenance.Create(assetId, DateOnly.FromDateTime(DateTime.Today), 500m, "Annual inspection", "VendorX");
        m.AssetId.Should().Be(assetId);
        m.Cost.Should().Be(500m);
        m.Description.Should().Be("Annual inspection");
        m.Vendor.Should().Be("VendorX");
    }

    [Fact]
    public void Create_WithNegativeCost_ShouldThrow()
    {
        var act = () => AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), -10m, "Test");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldThrow()
    {
        var act = () => AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 100m, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroCost_ShouldSucceed()
    {
        var m = AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 0m, "Free inspection");
        m.Cost.Should().Be(0m);
    }

    [Fact]
    public void SetExpenseId_ShouldUpdateExpenseId()
    {
        var m = AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 100m, "Test");
        var expenseId = Guid.NewGuid();
        m.SetExpenseId(expenseId);
        m.ExpenseId.Should().Be(expenseId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var m = AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 100m, "Test");
        m.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Create_ShouldTrimDescription()
    {
        var m = AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 100m, "  Repair  ");
        m.Description.Should().Be("Repair");
    }

    [Fact]
    public void Create_WithNullVendor_ShouldSucceed()
    {
        var m = AssetMaintenance.Create(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today), 100m, "Test", null);
        m.Vendor.Should().BeNull();
    }
}

