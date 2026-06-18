namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class TreatmentPlan
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal EstimatedCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal ActualCost { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string Priority { get; private set; } = "Normal"; // Low|Normal|High|Urgent
    public string Status { get; private set; } = "Draft";    // Draft|Active|Completed|Cancelled|OnHold
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<TreatmentPlanItem> Items { get; private set; } = [];

    private TreatmentPlan() { }

    public static TreatmentPlan Create(
        Guid patientId,
        Guid doctorId,
        string title,
        decimal estimatedCost,
        string priority = "Normal",
        string? description = null,
        string? notes = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        return new TreatmentPlan
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            Title = title,
            EstimatedCost = estimatedCost,
            Priority = priority,
            Description = description,
            Notes = notes,
            StartDate = startDate,
            EndDate = endDate,
            TotalCost = 0,
            ActualCost = 0,
            PaidAmount = 0,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        if (Status != "Draft" && Status != "OnHold")
            throw new InvalidOperationException($"Cannot activate plan in status '{Status}'.");
        Status = "Active";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != "Active")
            throw new InvalidOperationException("Only Active plans can be completed.");
        Status = "Completed";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == "Completed")
            throw new InvalidOperationException("Cannot cancel a completed plan.");
        Status = "Cancelled";
        UpdatedAt = DateTime.UtcNow;
    }

    public void PutOnHold()
    {
        if (Status != "Active")
            throw new InvalidOperationException("Only Active plans can be put on hold.");
        Status = "OnHold";
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecalculateTotalCost()
    {
        TotalCost = Items.Where(i => i.Status != "Cancelled").Sum(i => i.TotalPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddActualCost(decimal amount)
    {
        ActualCost += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (Status == "Active")
            throw new InvalidOperationException("Cannot delete an active plan.");
        DeletedAt = DateTime.UtcNow;
    }

    public static readonly string[] ValidStatuses = ["Draft", "Active", "Completed", "Cancelled", "OnHold"];
    public static readonly string[] ValidPriorities = ["Low", "Normal", "High", "Urgent"];
}
