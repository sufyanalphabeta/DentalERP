using DentalERP.Modules.Laboratory.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Laboratory;

public class LabOrderTests
{
    private static LabOrder CreateOrder(Guid? clientId = null)
    {
        return LabOrder.Create(
            "LAB-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            labId: Guid.NewGuid(),
            clientId: clientId);
    }

    private static LabResult MakeResult(Guid orderId) =>
        LabResult.Create(orderId, "Normal findings");

    [Fact]
    public void Create_SetsStatusToDraft()
    {
        var order = CreateOrder();
        order.Status.Should().Be("Draft");
    }

    [Fact]
    public void Create_WithClientId_IsExternal()
    {
        var order = CreateOrder(clientId: Guid.NewGuid());
        order.IsExternal.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutClientId_IsNotExternal()
    {
        var order = CreateOrder();
        order.IsExternal.Should().BeFalse();
    }

    [Fact]
    public void Send_FromDraft_SucceedsAndSetsSentStatus()
    {
        var order = CreateOrder();
        var result = order.Send();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Sent");
        order.SentAt.Should().NotBeNull();
    }

    [Fact]
    public void Send_FromNonDraft_Fails()
    {
        var order = CreateOrder();
        order.Send();
        var result = order.Send();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void MarkInProgress_FromSent_Succeeds()
    {
        var order = CreateOrder();
        order.Send();
        var result = order.MarkInProgress();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("InProgress");
    }

    [Fact]
    public void MarkInProgress_FromDraft_Fails()
    {
        var order = CreateOrder();
        var result = order.MarkInProgress();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void RecordResult_FromInProgress_Succeeds()
    {
        var order = CreateOrder();
        order.Send();
        order.MarkInProgress();
        var labResult = MakeResult(order.Id);
        var result = order.RecordResult(labResult);
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("ResultReceived");
        order.Results.Should().HaveCount(1);
    }

    [Fact]
    public void RecordResult_FromCompleted_Fails()
    {
        var order = CreateOrder();
        order.Send();
        order.MarkInProgress();
        order.RecordResult(MakeResult(order.Id));
        order.Complete();
        var result = order.RecordResult(MakeResult(order.Id));
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Complete_FromResultReceived_Succeeds()
    {
        var order = CreateOrder();
        order.Send();
        order.MarkInProgress();
        order.RecordResult(MakeResult(order.Id));
        var result = order.Complete();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Completed");
    }

    [Fact]
    public void Complete_FromSent_Fails()
    {
        var order = CreateOrder();
        order.Send();
        var result = order.Complete();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancel_FromDraft_Succeeds()
    {
        var order = CreateOrder();
        var result = order.Cancel("No longer needed");
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("Cancelled");
        order.CancelledReason.Should().Be("No longer needed");
    }

    [Fact]
    public void Cancel_FromSent_Fails()
    {
        var order = CreateOrder();
        order.Send();
        var result = order.Cancel("Too late");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AddItem_IncreasesItemCountAndRecalculates()
    {
        var order = CreateOrder();
        order.AddItem(LabOrderItem.Create(order.Id, "Crown", 250m, 2));
        order.Items.Should().HaveCount(1);
        order.TotalCost.Should().Be(500m);
    }

    [Fact]
    public void RecalculateCosts_SumsAllItems()
    {
        var order = CreateOrder();
        order.AddItem(LabOrderItem.Create(order.Id, "Item1", 100m, 2));
        order.AddItem(LabOrderItem.Create(order.Id, "Item2", 50m, 3));
        order.TotalCost.Should().Be(350m);
    }

    [Fact]
    public void SetRevenue_UpdatesTotalRevenue()
    {
        var order = CreateOrder();
        order.SetRevenue(500m);
        order.TotalRevenue.Should().Be(500m);
    }
}
