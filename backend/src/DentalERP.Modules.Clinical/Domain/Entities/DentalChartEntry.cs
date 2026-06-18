namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class DentalChartEntry
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public short ToothId { get; private set; }
    public string? Surface { get; private set; }   // null=whole, M|D|B|L|O
    public string Condition { get; private set; } = string.Empty;
    public string? Severity { get; private set; }  // Mild|Moderate|Severe
    public string? Notes { get; private set; }
    public Guid RecordedById { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public bool IsCurrent { get; private set; } = true;

    public Tooth? Tooth { get; private set; }

    private DentalChartEntry() { }

    public static DentalChartEntry Create(
        Guid patientId,
        short toothId,
        string condition,
        Guid recordedById,
        string? surface = null,
        string? severity = null,
        string? notes = null,
        Guid? appointmentId = null)
    {
        return new DentalChartEntry
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ToothId = toothId,
            Condition = condition,
            RecordedById = recordedById,
            Surface = surface,
            Severity = severity,
            Notes = notes,
            AppointmentId = appointmentId,
            RecordedAt = DateTime.UtcNow,
            IsCurrent = true
        };
    }

    public void MarkSuperseded() => IsCurrent = false;

    public static readonly string[] ValidConditions =
    [
        "Healthy", "Caries", "Filled", "Missing", "Extracted",
        "Crown", "Bridge", "Implant", "RootCanal", "Fracture",
        "Impacted", "Sensitive", "Mobility", "Other"
    ];

    public static readonly string[] ValidSurfaces = ["M", "D", "B", "L", "O"];
    public static readonly string[] ValidSeverities = ["Mild", "Moderate", "Severe"];
}
