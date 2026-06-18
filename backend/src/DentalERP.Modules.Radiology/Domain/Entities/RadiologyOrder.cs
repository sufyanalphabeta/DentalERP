using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Radiology.Domain.Entities;

public sealed class RadiologyOrder
{
    public static readonly string[] ValidStatuses =
        ["Ordered", "Imaged", "ReportSaved", "Completed", "Cancelled"];

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public Guid? PatientId { get; private set; }
    public bool IsExternalPatient { get; private set; }
    public string? ExternalPatientName { get; private set; }
    public string? ExternalPatientPhone { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public Guid RadiologyTypeId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public decimal Price { get; private set; }
    public decimal DoctorCommissionAmount { get; private set; }
    public decimal TechCommissionAmount { get; private set; }
    public string Status { get; private set; } = default!;
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public RadiologyType RadiologyType { get; private set; } = default!;
    public IReadOnlyList<RadiologyImage> Images => _images.AsReadOnly();
    public RadiologyReport? Report { get; private set; }

    private readonly List<RadiologyImage> _images = new();

    private RadiologyOrder() { }

    public static RadiologyOrder Create(
        string orderNumber,
        Guid radiologyTypeId,
        decimal price,
        Guid? patientId,
        bool isExternalPatient,
        string? externalPatientName,
        string? externalPatientPhone,
        Guid? doctorId,
        Guid? technicianId,
        string? notes)
    {
        if (!isExternalPatient && patientId == null)
            throw new ArgumentException("PatientId required for internal patients.");
        if (isExternalPatient && string.IsNullOrWhiteSpace(externalPatientName))
            throw new ArgumentException("ExternalPatientName required for external patients.");

        return new RadiologyOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            RadiologyTypeId = radiologyTypeId,
            Price = price,
            PatientId = patientId,
            IsExternalPatient = isExternalPatient,
            ExternalPatientName = externalPatientName,
            ExternalPatientPhone = externalPatientPhone,
            DoctorId = doctorId,
            TechnicianId = technicianId,
            Notes = notes,
            Status = "Ordered",
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Result MarkImaged()
    {
        if (Status != "Ordered")
            return Result.Failure(new Error("RadiologyOrder.InvalidStatus", "Only Ordered orders can be marked as imaged."));
        Status = "Imaged";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SaveReport(string reportText, Guid reportedById)
    {
        if (Status != "Imaged")
            return Result.Failure(new Error("RadiologyOrder.InvalidStatus", "Only Imaged orders can have a report saved."));
        Report = RadiologyReport.Create(Id, reportText, reportedById);
        Status = "ReportSaved";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateReport(string reportText)
    {
        if (Report == null)
            return Result.Failure(new Error("RadiologyOrder.NoReport", "No report exists to update."));
        Report.Update(reportText);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != "ReportSaved")
            return Result.Failure(new Error("RadiologyOrder.InvalidStatus", "Only ReportSaved orders can be completed."));
        Status = "Completed";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == "Completed" || Status == "Cancelled")
            return Result.Failure(new Error("RadiologyOrder.InvalidStatus", "Cannot cancel a completed or already cancelled order."));
        Status = "Cancelled";
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void AddImage(RadiologyImage image) => _images.Add(image);

    public void SetInvoice(Guid invoiceId)
    {
        InvoiceId = invoiceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCommissions(decimal doctorCommission, decimal techCommission)
    {
        DoctorCommissionAmount = doctorCommission;
        TechCommissionAmount = techCommission;
        UpdatedAt = DateTime.UtcNow;
    }
}
