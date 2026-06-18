using DentalERP.Modules.Inventory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Inventory;

public sealed class StockMovementTests
{
    private static readonly Guid ItemId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    [Fact]
    public void Create_PurchaseReceipt_Succeeds()
    {
        var result = StockMovement.Create("MOV-2026-000001", ItemId, WarehouseId,
            "PurchaseReceipt", "in", 50m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be("in");
        result.Value.MovementType.Should().Be("PurchaseReceipt");
    }

    [Fact]
    public void Create_ManualIssue_RequiresDestinationType()
    {
        var result = StockMovement.Create("MOV-2026-000002", ItemId, WarehouseId,
            "ManualIssue", "out", 10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("DestinationRequired");
    }

    [Fact]
    public void Create_ManualIssue_WithDestination_Succeeds()
    {
        var result = StockMovement.Create("MOV-2026-000003", ItemId, WarehouseId,
            "ManualIssue", "out", 10m, destinationType: "Clinic");

        result.IsSuccess.Should().BeTrue();
        result.Value.DestinationType.Should().Be("Clinic");
    }

    [Fact]
    public void Create_LabConsumption_AutoSetsDestination()
    {
        var result = StockMovement.Create("MOV-2026-000004", ItemId, WarehouseId,
            "LabConsumption", "out", 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.DestinationType.Should().Be("Lab");
    }

    [Fact]
    public void Create_RadiologyConsumption_AutoSetsDestination()
    {
        var result = StockMovement.Create("MOV-2026-000005", ItemId, WarehouseId,
            "RadiologyConsumption", "out", 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.DestinationType.Should().Be("Radiology");
    }

    [Fact]
    public void Create_ZeroQuantity_Fails()
    {
        var result = StockMovement.Create("MOV-2026-000006", ItemId, WarehouseId,
            "PurchaseReceipt", "in", 0m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidDirection_Fails()
    {
        var result = StockMovement.Create("MOV-2026-000007", ItemId, WarehouseId,
            "PurchaseReceipt", "sideways", 10m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidMovementType_Fails()
    {
        var result = StockMovement.Create("MOV-2026-000008", ItemId, WarehouseId,
            "Unknown", "in", 10m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_InvalidDestinationType_Fails()
    {
        var result = StockMovement.Create("MOV-2026-000009", ItemId, WarehouseId,
            "ManualIssue", "out", 10m, destinationType: "InvalidDest");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNegativeStockFlag_SetsFlag()
    {
        var result = StockMovement.Create("MOV-2026-000010", ItemId, WarehouseId,
            "ManualIssue", "out", 10m, destinationType: "Clinic", isNegativeStock: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsNegativeStock.Should().BeTrue();
    }

    [Fact]
    public void Create_SupplierReturn_Succeeds()
    {
        var result = StockMovement.Create("MOV-2026-000011", ItemId, WarehouseId,
            "SupplierReturn", "out", 5m);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_Adjustment_In_Succeeds()
    {
        var result = StockMovement.Create("MOV-2026-000012", ItemId, WarehouseId,
            "Adjustment", "in", 20m);

        result.IsSuccess.Should().BeTrue();
    }
}
