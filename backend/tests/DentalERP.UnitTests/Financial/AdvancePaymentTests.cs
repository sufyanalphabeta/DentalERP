using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class AdvancePaymentTests
{
    private static AdvancePayment CreateAdvance(decimal amount = 1000m) =>
        AdvancePayment.Create(Guid.NewGuid(), Guid.NewGuid(), amount);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_RemainingEqualsAmount()
    {
        var adv = CreateAdvance(500m);
        adv.Amount.Should().Be(500m);
        adv.Remaining.Should().Be(500m);
    }

    [Fact]
    public void Create_StoresNotes()
    {
        var adv = AdvancePayment.Create(Guid.NewGuid(), Guid.NewGuid(), 200m, "Deposit for treatment");
        adv.Notes.Should().Be("Deposit for treatment");
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var a1 = CreateAdvance();
        var a2 = CreateAdvance();
        a1.Id.Should().NotBe(a2.Id);
    }

    // ── Apply ────────────────────────────────────────────────────

    [Fact]
    public void Apply_ReducesRemaining()
    {
        var adv = CreateAdvance(1000m);
        adv.Apply(300m);
        adv.Remaining.Should().Be(700m);
    }

    [Fact]
    public void Apply_ExactAmount_RemainingIsZero()
    {
        var adv = CreateAdvance(200m);
        var result = adv.Apply(200m);
        result.IsSuccess.Should().BeTrue();
        adv.Remaining.Should().Be(0m);
    }

    [Fact]
    public void Apply_MultipleApplications_AccumulatesCorrectly()
    {
        var adv = CreateAdvance(900m);
        adv.Apply(300m);
        adv.Apply(300m);
        adv.Remaining.Should().Be(300m);
    }

    [Fact]
    public void Apply_ZeroAmount_Fails()
    {
        var adv = CreateAdvance(500m);
        var result = adv.Apply(0m);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("AdvancePayment.InvalidAmount");
    }

    [Fact]
    public void Apply_NegativeAmount_Fails()
    {
        var adv = CreateAdvance(500m);
        var result = adv.Apply(-50m);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("AdvancePayment.InvalidAmount");
    }

    [Fact]
    public void Apply_ExceedsRemaining_Fails()
    {
        var adv = CreateAdvance(100m);
        var result = adv.Apply(150m);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("AdvancePayment.Insufficient");
    }

    [Fact]
    public void Apply_DoesNotReduceAmount_OnlyRemaining()
    {
        var adv = CreateAdvance(500m);
        adv.Apply(100m);
        adv.Amount.Should().Be(500m); // original amount never changes
        adv.Remaining.Should().Be(400m);
    }
}
