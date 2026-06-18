namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class VaultTransaction
{
    public static readonly string[] ValidTypes =
    [
        "receipt_from_patient", "payment_to_doctor",
        "general_receipt", "general_payment", "inter_vault_transfer"
    ];

    public Guid Id { get; private set; }
    public Guid VaultId { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Direction { get; private set; } = string.Empty; // in|out
    public Guid? RelatedInvoiceId { get; private set; }
    public Guid? RelatedPatientId { get; private set; }
    public Guid? RelatedDoctorId { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public bool IsReversed { get; private set; }
    public bool IsReversal { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VaultTransaction() { }

    public static VaultTransaction Create(
        Guid vaultId,
        string transactionType,
        decimal amount,
        string direction,
        Guid? relatedInvoiceId = null,
        Guid? relatedPatientId = null,
        Guid? relatedDoctorId = null,
        string? referenceNumber = null,
        string? notes = null,
        bool isReversal = false,
        Guid? createdByUserId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            VaultId = vaultId,
            TransactionType = transactionType,
            Amount = amount,
            Direction = direction,
            RelatedInvoiceId = relatedInvoiceId,
            RelatedPatientId = relatedPatientId,
            RelatedDoctorId = relatedDoctorId,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            IsReversal = isReversal,
            IsReversed = false,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

    public void MarkReversed() => IsReversed = true;
}
