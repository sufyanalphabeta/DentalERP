using DentalERP.Modules.Purchasing.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Purchasing;

public sealed class PurchaseOrderTests
{
    private static PurchaseOrder CreateDraftPO()
        => PurchaseOrder.Create("PO-2026-000001", Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow));

    private static PurchaseOrderItem CreateItem(Guid poId)
        => PurchaseOrderItem.Create(poId, Guid.NewGuid(), 10m, 5m);

    [Fact]
    public void Create_SetsDraftStatus()
    {
        var po = CreateDraftPO();
        po.Status.Should().Be("Draft");
        po.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void AddItem_RecalculatesTotals()
    {
        var po = CreateDraftPO();
        po.AddItem(PurchaseOrderItem.Create(po.Id, Guid.NewGuid(), 10m, 5m));

        po.Subtotal.Should().Be(50m);
        po.TotalAmount.Should().Be(50m);
    }

    [Fact]
    public void AddItem_WithDiscount_ReducesTotal()
    {
        var po = PurchaseOrder.Create("PO-2026-000002", Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow), discountAmount: 10m);
        po.AddItem(PurchaseOrderItem.Create(po.Id, Guid.NewGuid(), 10m, 5m));

        po.Subtotal.Should().Be(50m);
        po.TotalAmount.Should().Be(40m);
    }

    [Fact]
    public void Approve_FromDraft_Succeeds()
    {
        var po = CreateDraftPO();
        po.AddItem(CreateItem(po.Id));
        var result = po.Approve(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be("Approved");
        po.ApprovedById.Should().NotBeNull();
        po.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_NoItems_Fails()
    {
        var po = CreateDraftPO();
        var result = po.Approve(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NoItems");
    }

    [Fact]
    public void Approve_AlreadyApproved_Fails()
    {
        var po = CreateDraftPO();
        po.AddItem(CreateItem(po.Id));
        po.Approve(Guid.NewGuid());
        var result = po.Approve(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MarkSent_FromApproved_Succeeds()
    {
        var po = CreateDraftPO();
        po.AddItem(CreateItem(po.Id));
        po.Approve(Guid.NewGuid());
        var result = po.MarkSent();

        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be("Sent");
    }

    [Fact]
    public void MarkSent_FromDraft_Fails()
    {
        var po = CreateDraftPO();
        var result = po.MarkSent();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_FromDraft_Succeeds()
    {
        var po = CreateDraftPO();
        var result = po.Cancel();
        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void Cancel_FromApproved_Succeeds()
    {
        var po = CreateDraftPO();
        po.AddItem(CreateItem(po.Id));
        po.Approve(Guid.NewGuid());
        var result = po.Cancel();
        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Fails()
    {
        var po = CreateDraftPO();
        po.Cancel();
        var result = po.Cancel();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateReceiptStatus_AllItemsReceived_SetsFullyReceived()
    {
        var po = CreateDraftPO();
        var item = PurchaseOrderItem.Create(po.Id, Guid.NewGuid(), 10m, 5m);
        po.AddItem(item);
        po.Approve(Guid.NewGuid());
        item.AddReceived(10m);
        po.UpdateReceiptStatus();

        po.Status.Should().Be("FullyReceived");
    }

    [Fact]
    public void UpdateReceiptStatus_PartialItems_SetsPartiallyReceived()
    {
        var po = CreateDraftPO();
        var item = PurchaseOrderItem.Create(po.Id, Guid.NewGuid(), 10m, 5m);
        po.AddItem(item);
        po.Approve(Guid.NewGuid());
        item.AddReceived(5m);
        po.UpdateReceiptStatus();

        po.Status.Should().Be("PartiallyReceived");
    }

    [Fact]
    public void PurchaseOrderItem_AddReceived_AccumulatesQuantity()
    {
        var item = PurchaseOrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 20m, 3m);
        item.AddReceived(5m);
        item.AddReceived(5m);
        item.QuantityReceived.Should().Be(10m);
    }
}
