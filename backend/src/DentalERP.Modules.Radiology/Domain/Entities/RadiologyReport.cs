namespace DentalERP.Modules.Radiology.Domain.Entities;

public sealed class RadiologyReport
{
    public Guid Id { get; private set; }
    public Guid RadiologyOrderId { get; private set; }
    public string ReportText { get; private set; } = default!;
    public Guid ReportedById { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public RadiologyOrder RadiologyOrder { get; private set; } = default!;

    private RadiologyReport() { }

    public static RadiologyReport Create(Guid orderId, string reportText, Guid reportedById)
    {
        return new RadiologyReport
        {
            Id = Guid.NewGuid(),
            RadiologyOrderId = orderId,
            ReportText = reportText,
            ReportedById = reportedById,
            ReportedAt = DateTime.UtcNow
        };
    }

    public void Update(string reportText)
    {
        ReportText = reportText;
        UpdatedAt = DateTime.UtcNow;
    }
}
