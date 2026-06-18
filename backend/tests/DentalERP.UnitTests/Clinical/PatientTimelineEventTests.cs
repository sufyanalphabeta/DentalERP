using DentalERP.Modules.Clinical.Domain.Entities;
using FluentAssertions;

namespace DentalERP.UnitTests.Clinical;

public class PatientTimelineEventTests
{
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public void Create_SetsDefaultVisibility()
    {
        var evt = PatientTimelineEvent.Create(PatientId, "chart.updated", "تحديث المخطط", "Clinical");

        evt.IsVisibleToDoctor.Should().BeTrue();
        evt.IsVisibleToPatient.Should().BeFalse();
    }

    [Fact]
    public void Create_SetsEventCategory()
    {
        var evt = PatientTimelineEvent.Create(PatientId, "media.uploaded", "رفع أشعة", "Radiology");

        evt.EventCategory.Should().Be("Radiology");
    }

    [Fact]
    public void Create_SetsEventAt_ToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var evt = PatientTimelineEvent.Create(PatientId, "appointment.scheduled", "حجز موعد", "Administrative");

        evt.EventAt.Should().BeAfter(before);
    }

    [Fact]
    public void EventTypes_ContainsAllPhase3Events()
    {
        var phase3Events = new[]
        {
            PatientTimelineEvent.EventTypes.ChartUpdated,
            PatientTimelineEvent.EventTypes.ProcedurePerformed,
            PatientTimelineEvent.EventTypes.TreatmentPlanCreated,
            PatientTimelineEvent.EventTypes.TreatmentPlanActivated,
            PatientTimelineEvent.EventTypes.TreatmentPlanCompleted,
            PatientTimelineEvent.EventTypes.MediaUploaded,
            PatientTimelineEvent.EventTypes.DoctorAssigned,
            PatientTimelineEvent.EventTypes.DoctorTransferred
        };

        phase3Events.Should().AllSatisfy(e => e.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void Categories_ContainsAllExpected()
    {
        var categories = new[]
        {
            PatientTimelineEvent.Categories.Clinical,
            PatientTimelineEvent.Categories.Financial,
            PatientTimelineEvent.Categories.Administrative,
            PatientTimelineEvent.Categories.Insurance,
            PatientTimelineEvent.Categories.Radiology,
            PatientTimelineEvent.Categories.Laboratory
        };

        categories.Should().AllSatisfy(c => c.Should().NotBeNullOrEmpty());
    }
}
