using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Patients.Domain.Entities;

public sealed class Appointment : BaseEntity
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? AppointmentTypeId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
    public string? ChiefComplaint { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid? CreatedById { get; private set; }

    public Patient? Patient { get; private set; }
    public AppointmentType? AppointmentType { get; private set; }

    private Appointment() { }

    public static Appointment Create(
        Guid patientId,
        Guid doctorId,
        DateTime scheduledAt,
        int durationMinutes,
        Guid? appointmentTypeId = null,
        string? chiefComplaint = null,
        string? notes = null,
        Guid? createdById = null)
        => new()
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            AppointmentTypeId = appointmentTypeId,
            ChiefComplaint = chiefComplaint,
            Notes = notes,
            CreatedById = createdById
        };

    public void Confirm() { Status = AppointmentStatus.Confirmed; Touch(); }
    public void Start() { Status = AppointmentStatus.InProgress; Touch(); }
    public void Complete() { Status = AppointmentStatus.Completed; Touch(); }
    public void Cancel(string? reason = null) { Status = AppointmentStatus.Cancelled; CancellationReason = reason; Touch(); }
    public void MarkNoShow() { Status = AppointmentStatus.NoShow; Touch(); }

    public void Reschedule(DateTime newDateTime, int? durationMinutes = null)
    {
        ScheduledAt = newDateTime;
        if (durationMinutes.HasValue) DurationMinutes = durationMinutes.Value;
        Status = AppointmentStatus.Scheduled;
        Touch();
    }

    public void UpdateNotes(string? chiefComplaint, string? notes)
    {
        ChiefComplaint = chiefComplaint;
        Notes = notes;
        Touch();
    }

    public DateTime EndsAt => ScheduledAt.AddMinutes(DurationMinutes);
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}
