namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class DoctorAssignment
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public string Status { get; private set; } = "Active"; // Active|Completed|Transferred|Closed
    public DateTime AssignedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public bool CanView { get; private set; } = true;
    public bool CanEdit { get; private set; } = true;
    public Guid? TransferredToId { get; private set; }
    public DateTime? TransferredAt { get; private set; }
    public string? TransferReason { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? Notes { get; private set; }
    public Guid? AssignedById { get; private set; }

    private DoctorAssignment() { }

    public static DoctorAssignment Create(
        Guid patientId,
        Guid doctorId,
        bool isPrimary = false,
        string? notes = null,
        Guid? assignedById = null)
    {
        return new DoctorAssignment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            Status = "Active",
            CanView = true,
            CanEdit = true,
            IsPrimary = isPrimary,
            Notes = notes,
            AssignedById = assignedById,
            AssignedAt = DateTime.UtcNow
        };
    }

    public DoctorAssignment Transfer(Guid newDoctorId, string? reason = null, Guid? transferredBy = null)
    {
        if (Status != "Active")
            throw new InvalidOperationException("Only Active assignments can be transferred.");

        Status = "Transferred";
        CanEdit = false;
        EndedAt = DateTime.UtcNow;
        TransferredToId = newDoctorId;
        TransferredAt = DateTime.UtcNow;
        TransferReason = reason;

        return Create(PatientId, newDoctorId, IsPrimary, assignedById: transferredBy);
    }

    public void Complete()
    {
        if (Status != "Active")
            throw new InvalidOperationException("Only Active assignments can be completed.");
        Status = "Completed";
        CanEdit = false;
        EndedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = "Closed";
        CanView = false;
        CanEdit = false;
        EndedAt = DateTime.UtcNow;
    }

    public static readonly string[] ValidStatuses = ["Active", "Completed", "Transferred", "Closed"];
}
