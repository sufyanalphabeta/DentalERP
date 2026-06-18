using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InsuranceClaim
{
    public static readonly string[] ValidStatuses =
        ["Draft", "Submitted", "PartiallyPaid", "FullyPaid", "Rejected"];

    public Guid Id { get; private set; }
    public string ClaimNumber { get; private set; } = default!;
    public Guid InvoiceId { get; private set; }
    public Guid InsuranceCompanyId { get; private set; }
    public Guid PatientId { get; private set; }
    public decimal ClaimedAmount { get; private set; }
    public decimal CoveragePercent { get; private set; }
    public decimal PaidAmount { get; private set; }
    public string Status { get; private set; } = default!;
    public string? RejectionReason { get; private set; }
    public string? Notes { get; private set; }
    public DateTime ClaimDate { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public InsuranceCompany InsuranceCompany { get; private set; } = default!;
    public IReadOnlyList<InsurancePayment> Payments => _payments.AsReadOnly();

    private readonly List<InsurancePayment> _payments = new();

    private InsuranceClaim() { }

    public static InsuranceClaim Create(string claimNumber, Guid invoiceId, Guid insuranceCompanyId,
        Guid patientId, decimal claimedAmount, decimal coveragePercent, string? notes)
    {
        return new InsuranceClaim
        {
            Id = Guid.NewGuid(),
            ClaimNumber = claimNumber,
            InvoiceId = invoiceId,
            InsuranceCompanyId = insuranceCompanyId,
            PatientId = patientId,
            ClaimedAmount = claimedAmount,
            CoveragePercent = coveragePercent,
            PaidAmount = 0,
            Status = "Draft",
            Notes = notes,
            ClaimDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Result Submit()
    {
        if (Status != "Draft")
            return Result.Failure(new Error("InsuranceClaim.InvalidStatus", "Only Draft claims can be submitted."));
        Status = "Submitted";
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RecordPayment(InsurancePayment payment)
    {
        if (Status == "Rejected" || Status == "FullyPaid")
            return Result.Failure(new Error("InsuranceClaim.InvalidStatus", "Cannot record payment on a rejected or fully paid claim."));

        _payments.Add(payment);
        PaidAmount += payment.Amount;
        Status = PaidAmount >= ClaimedAmount ? "FullyPaid" : "PartiallyPaid";
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Reject(string reason)
    {
        if (Status != "Submitted")
            return Result.Failure(new Error("InsuranceClaim.InvalidStatus", "Only Submitted claims can be rejected."));
        Status = "Rejected";
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
