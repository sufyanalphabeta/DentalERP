using DentalERP.Modules.Inventory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Inventory;

public sealed class ItemTests
{
    [Fact]
    public void Create_ValidItem_SetsProperties()
    {
        var item = Item.Create("ITM-000001", "Gloves", unitCost: 5.50m, allowNegativeStock: false);

        item.ItemCode.Should().Be("ITM-000001");
        item.Name.Should().Be("Gloves");
        item.UnitCost.Should().Be(5.50m);
        item.AllowNegativeStock.Should().BeFalse();
        item.IsDeleted.Should().BeFalse();
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNegativeCost_Throws()
    {
        var act = () => Item.Create("ITM-000002", "Test", unitCost: -1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_AllowNegativeStock_True_WhenSet()
    {
        var item = Item.Create("ITM-000003", "Syringe", allowNegativeStock: true);
        item.AllowNegativeStock.Should().BeTrue();
    }

    [Fact]
    public void Create_DefaultAllowNegativeStock_IsFalse()
    {
        var item = Item.Create("ITM-000004", "Cotton", unitCost: 0m);
        item.AllowNegativeStock.Should().BeFalse();
    }

    [Fact]
    public void AddBarcode_AddsToCollection()
    {
        var item = Item.Create("ITM-000005", "Mask", unitCost: 1m);
        var barcode = ItemBarcode.Create(item.Id, "BC-001");
        var result = item.AddBarcode(barcode);

        result.IsSuccess.Should().BeTrue();
        item.Barcodes.Should().ContainSingle(b => b.Barcode == "BC-001");
    }

    [Fact]
    public void AddBarcode_Duplicate_ReturnsFailure()
    {
        var item = Item.Create("ITM-000006", "Mask", unitCost: 1m);
        var bc1 = ItemBarcode.Create(item.Id, "BC-DUP");
        var bc2 = ItemBarcode.Create(item.Id, "BC-DUP");
        item.AddBarcode(bc1);
        var result = item.AddBarcode(bc2);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoveBarcode_RemovesFromCollection()
    {
        var item = Item.Create("ITM-000007", "Mask", unitCost: 1m);
        var bc = ItemBarcode.Create(item.Id, "BC-REMOVE");
        item.AddBarcode(bc);
        item.RemoveBarcode(bc.Id);
        item.Barcodes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateCost_ChangesCost()
    {
        var item = Item.Create("ITM-000008", "Mask", unitCost: 1m);
        item.UpdateCost(3.50m);
        item.UnitCost.Should().Be(3.50m);
    }

    [Fact]
    public void UpdateCost_Negative_Throws()
    {
        var item = Item.Create("ITM-000009", "Mask", unitCost: 1m);
        var act = () => item.UpdateCost(-0.01m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SoftDelete_SetsDeletedAtAndIsDeleted()
    {
        var item = Item.Create("ITM-000010", "Mask", unitCost: 1m);
        item.Delete();
        item.IsDeleted.Should().BeTrue();
        item.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ChangesNameAndProperties()
    {
        var item = Item.Create("ITM-000011", "Original", unitCost: 1m);
        item.Update("Updated", null, null, null, 5m, 10m, false, false, null, null);
        item.Name.Should().Be("Updated");
        item.ReorderLevel.Should().Be(5m);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var item = Item.Create("ITM-000012", "Gauze", unitCost: 1m);
        item.Deactivate();
        item.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var item = Item.Create("ITM-000013", "Gauze", unitCost: 1m);
        item.Deactivate();
        item.Activate();
        item.IsActive.Should().BeTrue();
    }
}
