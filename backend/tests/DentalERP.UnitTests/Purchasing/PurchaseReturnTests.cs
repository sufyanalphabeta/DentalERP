using DentalERP.Modules.Purchasing.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Purchasing;

public sealed class PurchaseReturnTests
{
    private static PurchaseReturn CreateReturn()
        => PurchaseReturn.Create("PRN-2026-000001", Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow), "Defective goods");

    [Fact]
    public void Create_SetsDraftStatus()
    {
        var ret = CreateReturn();
        ret.Status.Should().Be("Draft");
        ret.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void Create_EmptyReason_Throws()
    {
        var act = () => PurchaseReturn.Create("PRN-2026-000002", Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow), reason: "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_RecalculatesTotal()
    {
        var ret = CreateReturn();
        ret.AddItem(PurchaseReturnItem.Create(ret.Id, Guid.NewGuid(), 5m, 10m));
        ret.TotalAmount.Should().Be(50m);
    }

    [Fact]
    public void Confirm_WithItems_Succeeds()
    {
        var ret = CreateReturn();
        ret.AddItem(PurchaseReturnItem.Create(ret.Id, Guid.NewGuid(), 2m, 15m));
        var result = ret.Confirm();

        result.IsSuccess.Should().BeTrue();
        ret.Status.Should().Be("Confirmed");
    }

    [Fact]
    public void Confirm_NoItems_Fails()
    {
        var ret = CreateReturn();
        var result = ret.Confirm();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NoItems");
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_Fails()
    {
        var ret = CreateReturn();
        ret.AddItem(PurchaseReturnItem.Create(ret.Id, Guid.NewGuid(), 1m, 10m));
        ret.Confirm();
        var result = ret.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void PurchaseReturnItem_Create_SetsProperties()
    {
        var returnId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = PurchaseReturnItem.Create(returnId, itemId, 3m, 20m);

        item.ReturnId.Should().Be(returnId);
        item.ItemId.Should().Be(itemId);
        item.Quantity.Should().Be(3m);
        item.UnitCost.Should().Be(20m);
        item.TotalCost.Should().Be(60m);
    }

    [Fact]
    public void PurchaseReturnItem_ZeroQuantity_Throws()
    {
        var act = () => PurchaseReturnItem.Create(Guid.NewGuid(), Guid.NewGuid(), 0m, 10m);
        act.Should().Throw<ArgumentException>();
    }
}
