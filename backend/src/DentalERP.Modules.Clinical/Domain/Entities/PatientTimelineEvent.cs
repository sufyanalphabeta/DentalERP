namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class PatientTimelineEvent
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ActorId { get; private set; }
    public string? ActorName { get; private set; }   // snapshot — survives user deletion
    public string? LinkedEntityType { get; private set; }
    public Guid? LinkedEntityId { get; private set; }
    public string? Metadata { get; private set; }    // JSON string
    public DateTime EventAt { get; private set; }
    public bool IsVisibleToDoctor { get; private set; } = true;
    public bool IsVisibleToPatient { get; private set; }
    public string EventCategory { get; private set; } = "Administrative";
    // Clinical|Financial|Administrative|Insurance|Radiology|Laboratory

    private PatientTimelineEvent() { }

    public static PatientTimelineEvent Create(
        Guid patientId,
        string eventType,
        string title,
        string eventCategory = "Administrative",
        string? description = null,
        Guid? actorId = null,
        string? actorName = null,
        string? linkedEntityType = null,
        Guid? linkedEntityId = null,
        string? metadata = null,
        bool isVisibleToDoctor = true,
        bool isVisibleToPatient = false)
    {
        return new PatientTimelineEvent
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            EventType = eventType,
            Title = title,
            EventCategory = eventCategory,
            Description = description,
            ActorId = actorId,
            ActorName = actorName,
            LinkedEntityType = linkedEntityType,
            LinkedEntityId = linkedEntityId,
            Metadata = metadata,
            IsVisibleToDoctor = isVisibleToDoctor,
            IsVisibleToPatient = isVisibleToPatient,
            EventAt = DateTime.UtcNow
        };
    }

    public static class EventTypes
    {
        // Phase 2
        public const string PatientRegistered = "patient.registered";
        public const string PatientUpdated = "patient.updated";
        public const string AppointmentScheduled = "appointment.scheduled";
        public const string AppointmentConfirmed = "appointment.confirmed";
        public const string AppointmentCompleted = "appointment.completed";
        public const string AppointmentCancelled = "appointment.cancelled";
        public const string AppointmentNoShow = "appointment.noshow";
        public const string QueueCheckIn = "queue.checkin";
        public const string QueueCalled = "queue.called";
        public const string QueueCompleted = "queue.completed";
        // Phase 3
        public const string ChartUpdated = "chart.updated";
        public const string ProcedurePerformed = "procedure.performed";
        public const string TreatmentPlanCreated = "treatment_plan.created";
        public const string TreatmentPlanActivated = "treatment_plan.activated";
        public const string TreatmentPlanCompleted = "treatment_plan.completed";
        public const string MediaUploaded = "media.uploaded";
        public const string DoctorAssigned = "doctor.assigned";
        public const string DoctorTransferred = "doctor.transferred";
    }

    public static class Categories
    {
        public const string Clinical = "Clinical";
        public const string Financial = "Financial";
        public const string Administrative = "Administrative";
        public const string Insurance = "Insurance";
        public const string Radiology = "Radiology";
        public const string Laboratory = "Laboratory";
    }
}
