using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Patients.Domain.Entities;

public sealed class QueueEntry : BaseEntity
{
    public Guid? AppointmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? DoctorId { get; private set; }
    public DateOnly QueueDate { get; private set; }
    public int TokenNumber { get; private set; }
    public QueueStatus Status { get; private set; } = QueueStatus.Waiting;
    public DateTime CheckInAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CalledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    public Patient? Patient { get; private set; }
    public Appointment? Appointment { get; private set; }

    private QueueEntry() { }

    public static QueueEntry Create(
        Guid patientId,
        int tokenNumber,
        DateOnly? queueDate = null,
        Guid? appointmentId = null,
        Guid? doctorId = null,
        string? notes = null)
        => new()
        {
            PatientId = patientId,
            TokenNumber = tokenNumber,
            QueueDate = queueDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AppointmentId = appointmentId,
            DoctorId = doctorId,
            Notes = notes
        };

    public void Call() { Status = QueueStatus.Called; CalledAt = DateTime.UtcNow; Touch(); }
    public void Start() { Status = QueueStatus.InProgress; StartedAt = DateTime.UtcNow; Touch(); }
    public void Complete() { Status = QueueStatus.Completed; CompletedAt = DateTime.UtcNow; Touch(); }
    public void Skip() { Status = QueueStatus.Skipped; Touch(); }
    public void ResetToWaiting() { Status = QueueStatus.Waiting; CalledAt = null; StartedAt = null; Touch(); }
}

public enum QueueStatus
{
    Waiting,
    Called,
    InProgress,
    Completed,
    Skipped
}
