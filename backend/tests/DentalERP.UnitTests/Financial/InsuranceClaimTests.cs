using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public class InsuranceClaimTests
{
    private static InsuranceClaim CreateClaim()
    {
        return InsuranceClaim.Create(
            "INS-2026-000001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000m,
            80m,
            null);
    }

    [Fact]
    public void Create_SetsStatusToDraft()
    {
        var claim = CreateClaim();
        claim.Status.Should().Be("Draft");
        claim.PaidAmount.Should().Be(0);
    }

    [Fact]
    public void Submit_FromDraft_Succeeds()
    {
        var claim = CreateClaim();
        var result = claim.Submit();
        result.IsSuccess.Should().BeTrue();
        claim.Status.Should().Be("Submitted");
        claim.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public void Submit_FromNonDraft_Fails()
    {
        var claim = CreateClaim();
        claim.Submit();
        var result = claim.Submit();
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("InsuranceClaim.InvalidStatus");
    }

    [Fact]
    public void RecordPayment_PartialAmount_SetsPartiallyPaid()
    {
        var claim = CreateClaim();
        claim.Submit();
        var payment = InsurancePayment.Create(claim.Id, 500m, null, null, Guid.NewGuid());
        var result = claim.RecordPayment(payment);
        result.IsSuccess.Should().BeTrue();
        claim.Status.Should().Be("PartiallyPaid");
        claim.PaidAmount.Should().Be(500m);
    }

    [Fact]
    public void RecordPayment_FullAmount_SetsFullyPaid()
    {
        var claim = CreateClaim();
        claim.Submit();
        var payment = InsurancePayment.Create(claim.Id, 1000m, null, null, Guid.NewGuid());
        claim.RecordPayment(payment);
        claim.Status.Should().Be("FullyPaid");
        claim.PaidAmount.Should().Be(1000m);
    }

    [Fact]
    public void RecordPayment_WhenRejected_Fails()
    {
        var claim = CreateClaim();
        claim.Submit();
        claim.Reject("Invalid");
        var payment = InsurancePayment.Create(claim.Id, 500m, null, null, Guid.NewGuid());
        var result = claim.RecordPayment(payment);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Reject_FromSubmitted_Succeeds()
    {
        var claim = CreateClaim();
        claim.Submit();
        var result = claim.Reject("Policy expired");
        result.IsSuccess.Should().BeTrue();
        claim.Status.Should().Be("Rejected");
        claim.RejectionReason.Should().Be("Policy expired");
    }

    [Fact]
    public void Reject_FromDraft_Fails()
    {
        var claim = CreateClaim();
        var result = claim.Reject("Not submitted yet");
        result.IsSuccess.Should().BeFalse();
    }
}
