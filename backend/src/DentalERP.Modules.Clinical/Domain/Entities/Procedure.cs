namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class Procedure
{
    public Guid Id { get; private set; }
    public Guid AppointmentId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? TreatmentPlanItemId { get; private set; }
    public short? ToothId { get; private set; }
    public string? Surface { get; private set; }
    public string ProcedureName { get; private set; } = string.Empty;
    public string? ProcedureCode { get; private set; }
    public Guid? ServiceId { get; private set; }     // NULL until Phase 5 services catalog
    public string? Notes { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string BillingStatus { get; private set; } = "Pending"; // Pending|SentToTreasury|Paid|Cancelled
    public DateTime CreatedAt { get; private set; }

    private Procedure() { }

    public static Procedure Create(
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        string procedureName,
        short? toothId = null,
        string? surface = null,
        string? procedureCode = null,
        Guid? treatmentPlanItemId = null,
        Guid? serviceId = null,
        string? notes = null,
        int? durationMinutes = null)
    {
        return new Procedure
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            ProcedureName = procedureName,
            ToothId = toothId,
            Surface = surface,
            ProcedureCode = procedureCode,
            TreatmentPlanItemId = treatmentPlanItemId,
            ServiceId = serviceId,
            Notes = notes,
            DurationMinutes = durationMinutes,
            BillingStatus = "Pending",
            PerformedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SendToTreasury() => BillingStatus = "SentToTreasury";
    public void MarkPaid() => BillingStatus = "Paid";
    public void CancelBilling() => BillingStatus = "Cancelled";

    public static readonly string[] ValidBillingStatuses = ["Pending", "SentToTreasury", "Paid", "Cancelled"];
}
