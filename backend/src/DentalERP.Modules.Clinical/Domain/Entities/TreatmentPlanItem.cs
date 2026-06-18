namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class TreatmentPlanItem
{
    public Guid Id { get; private set; }
    public Guid TreatmentPlanId { get; private set; }
    public short? ToothId { get; private set; }
    public string? Surface { get; private set; }
    public string ProcedureName { get; private set; } = string.Empty;
    public string? ProcedureCode { get; private set; }
    public int Quantity { get; private set; } = 1;
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice * (1 - DiscountPercent / 100);
    public string Status { get; private set; } = "Pending"; // Pending|InProgress|Completed|Cancelled
    public int SequenceNumber { get; private set; } = 1;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TreatmentPlanItem() { }

    public static TreatmentPlanItem Create(
        Guid treatmentPlanId,
        string procedureName,
        decimal unitPrice,
        int quantity = 1,
        decimal discountPercent = 0,
        short? toothId = null,
        string? surface = null,
        string? procedureCode = null,
        int sequenceNumber = 1,
        string? notes = null)
    {
        return new TreatmentPlanItem
        {
            Id = Guid.NewGuid(),
            TreatmentPlanId = treatmentPlanId,
            ProcedureName = procedureName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            DiscountPercent = discountPercent,
            ToothId = toothId,
            Surface = surface,
            ProcedureCode = procedureCode,
            SequenceNumber = sequenceNumber,
            Notes = notes,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkInProgress() => Status = "InProgress";
    public void MarkCompleted() => Status = "Completed";
    public void Cancel() => Status = "Cancelled";
}
