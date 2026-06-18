using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class TreatmentPlanTests
{
    private static readonly Guid PatientId = Guid.NewGuid();
    private static readonly Guid DoctorId = Guid.NewGuid();

    [Fact]
    public void Create_DefaultsToStatus_Draft()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة التقويم", 5000);

        plan.Status.Should().Be("Draft");
        plan.Priority.Should().Be("Normal");
    }

    [Fact]
    public void Create_WithHighPriority_SetsPriority()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة طارئة", 3000, priority: "Urgent");

        plan.Priority.Should().Be("Urgent");
    }

    [Fact]
    public void Activate_FromDraft_ChangesStatusToActive()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);

        plan.Activate();

        plan.Status.Should().Be("Active");
    }

    [Fact]
    public void Activate_FromCompleted_ThrowsInvalidOperation()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);
        plan.Activate();
        plan.Complete();

        plan.Invoking(p => p.Activate())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_FromActive_ChangesStatusToCompleted()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);
        plan.Activate();

        plan.Complete();

        plan.Status.Should().Be("Completed");
    }

    [Fact]
    public void Cancel_FromDraft_Succeeds()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);

        plan.Cancel();

        plan.Status.Should().Be("Cancelled");
    }

    [Fact]
    public void Cancel_FromCompleted_ThrowsInvalidOperation()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);
        plan.Activate();
        plan.Complete();

        plan.Invoking(p => p.Cancel())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PutOnHold_FromActive_ChangesStatus()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 2000);
        plan.Activate();

        plan.PutOnHold();

        plan.Status.Should().Be("OnHold");
    }

    [Fact]
    public void RecalculateTotalCost_SumsCancelledExcluded()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 5000);
        plan.Items.Add(TreatmentPlanItem.Create(plan.Id, "حشو", 300, 1));
        plan.Items.Add(TreatmentPlanItem.Create(plan.Id, "تنظيف", 200, 1));
        var cancelled = TreatmentPlanItem.Create(plan.Id, "طرطرة", 100, 1);
        cancelled.Cancel();
        plan.Items.Add(cancelled);

        plan.RecalculateTotalCost();

        plan.TotalCost.Should().Be(500); // 300 + 200, excluded 100 cancelled
    }

    [Fact]
    public void AddActualCost_AccumulatesCorrectly()
    {
        var plan = TreatmentPlan.Create(PatientId, DoctorId, "خطة", 5000);

        plan.AddActualCost(200);
        plan.AddActualCost(300);

        plan.ActualCost.Should().Be(500);
    }
}
