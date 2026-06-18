using DentalERP.Modules.Financial.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Financial;

public sealed class InstallmentPlanTests
{
    private static readonly DateTime StartDate = new(2024, 1, 1);

    private static InstallmentPlan CreatePlan(
        decimal total = 1200m,
        short count = 3,
        DateTime? start = null)
        => InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), total, count,
            start ?? StartDate);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_GeneratesCorrectNumberOfInstallments()
    {
        var plan = CreatePlan(count: 6);

        plan.Installments.Should().HaveCount(6);
    }

    [Fact]
    public void Create_InstallmentsHaveCorrectAmount()
    {
        var plan = CreatePlan(total: 900m, count: 3);

        foreach (var inst in plan.Installments)
            inst.Amount.Should().Be(300m);
    }

    [Fact]
    public void Create_AmountRoundedToTwoDecimals()
    {
        // 100 / 3 = 33.33...
        var plan = CreatePlan(total: 100m, count: 3);

        foreach (var inst in plan.Installments)
            inst.Amount.Should().Be(33.33m);
    }

    [Fact]
    public void Create_InstallmentsNumberedSequentially()
    {
        var plan = CreatePlan(count: 4);

        plan.Installments.Select(i => (int)i.InstallmentNum)
            .Should().BeEquivalentTo([1, 2, 3, 4]);
    }

    [Fact]
    public void Create_DueDatesAreMonthlyFromStart()
    {
        var plan = CreatePlan(count: 3, start: new DateTime(2024, 1, 1));

        var dates = plan.Installments.Select(i => i.DueDate).ToList();
        dates[0].Should().Be(new DateOnly(2024, 1, 1));
        dates[1].Should().Be(new DateOnly(2024, 2, 1));
        dates[2].Should().Be(new DateOnly(2024, 3, 1));
    }

    [Fact]
    public void Create_AllInstallmentsStatusIsPending()
    {
        var plan = CreatePlan(count: 3);

        plan.Installments.Should().AllSatisfy(i => i.Status.Should().Be("Pending"));
    }

    [Fact]
    public void Create_StoresCorrectTotalAmount()
    {
        var plan = CreatePlan(total: 1500m, count: 5);

        plan.TotalAmount.Should().Be(1500m);
    }

    [Fact]
    public void Create_StoresInstallmentsCount()
    {
        var plan = CreatePlan(count: 12);

        plan.InstallmentsCount.Should().Be(12);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        var plan1 = CreatePlan();
        var plan2 = CreatePlan();

        plan1.Id.Should().NotBe(plan2.Id);
    }
}

public sealed class InstallmentPaymentTests
{
    private static InstallmentPlan CreatePlan(short count = 3) =>
        InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, count,
            new DateTime(2024, 1, 1));

    // ── Pay ─────────────────────────────────────────────────────

    [Fact]
    public void Pay_PendingInstallment_Succeeds()
    {
        var plan = CreatePlan();
        var inst = plan.Installments[0];
        var vaultId = Guid.NewGuid();

        var result = inst.Pay(vaultId, "cash");

        result.IsSuccess.Should().BeTrue();
        inst.Status.Should().Be("Paid");
        inst.VaultId.Should().Be(vaultId);
        inst.PaymentMethod.Should().Be("cash");
        inst.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void Pay_AlreadyPaidInstallment_Fails()
    {
        var plan = CreatePlan();
        var inst = plan.Installments[0];
        inst.Pay(Guid.NewGuid(), "cash");

        var result = inst.Pay(Guid.NewGuid(), "card");

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Installment.AlreadyPaid");
    }

    [Fact]
    public void Pay_OverdueInstallment_Succeeds()
    {
        var pastPlan = InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, 1,
            new DateTime(2020, 1, 1));
        var inst = pastPlan.Installments[0];
        inst.MarkOverdue();

        var result = inst.Pay(Guid.NewGuid(), "bank_transfer");

        result.IsSuccess.Should().BeTrue();
        inst.Status.Should().Be("Paid");
    }

    // ── MarkOverdue ──────────────────────────────────────────────

    [Fact]
    public void MarkOverdue_PendingAndPastDue_SetsOverdue()
    {
        var pastPlan = InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, 1,
            new DateTime(2020, 1, 1));
        var inst = pastPlan.Installments[0];

        inst.MarkOverdue();

        inst.Status.Should().Be("Overdue");
    }

    [Fact]
    public void MarkOverdue_PendingAndFutureDue_StaysPending()
    {
        var futurePlan = InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, 1,
            DateTime.UtcNow.AddYears(1));
        var inst = futurePlan.Installments[0];

        inst.MarkOverdue();

        inst.Status.Should().Be("Pending");
    }

    [Fact]
    public void MarkOverdue_PaidInstallment_StaysPaid()
    {
        var pastPlan = InstallmentPlan.Create(
            Guid.NewGuid(), Guid.NewGuid(), 300m, 1,
            new DateTime(2020, 1, 1));
        var inst = pastPlan.Installments[0];
        inst.Pay(Guid.NewGuid(), "cash");

        inst.MarkOverdue();

        inst.Status.Should().Be("Paid");
    }
}
