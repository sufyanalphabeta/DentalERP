using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class TreatmentPlanItemTests
{
    private static readonly Guid PlanId = Guid.NewGuid();

    [Fact]
    public void Create_CalculatesTotalPrice_Correctly()
    {
        var item = TreatmentPlanItem.Create(PlanId, "حشو", 500, quantity: 2, discountPercent: 10);

        item.TotalPrice.Should().Be(900); // 2 * 500 * (1 - 10/100) = 900
    }

    [Fact]
    public void Create_NoDiscount_TotalPriceEqualsQuantityTimesUnit()
    {
        var item = TreatmentPlanItem.Create(PlanId, "تلبيس", 1200, quantity: 1);

        item.TotalPrice.Should().Be(1200);
    }

    [Fact]
    public void Create_FullDiscount_TotalPriceIsZero()
    {
        var item = TreatmentPlanItem.Create(PlanId, "فحص", 100, quantity: 1, discountPercent: 100);

        item.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void MarkCompleted_ChangesStatus()
    {
        var item = TreatmentPlanItem.Create(PlanId, "حشو", 300);

        item.MarkCompleted();

        item.Status.Should().Be("Completed");
    }

    [Fact]
    public void Cancel_ChangesStatus()
    {
        var item = TreatmentPlanItem.Create(PlanId, "حشو", 300);

        item.Cancel();

        item.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void Create_DefaultsToStatusPending()
    {
        var item = TreatmentPlanItem.Create(PlanId, "حشو", 300);

        item.Status.Should().Be("Pending");
    }
}
