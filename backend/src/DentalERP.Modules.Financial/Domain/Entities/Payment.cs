namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class Payment
{
    public static readonly string[] ValidMethods =
        ["cash", "bank_transfer", "card", "pos", "cheque"];

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid VaultId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid invoiceId,
        Guid vaultId,
        decimal amount,
        string paymentMethod,
        string? referenceNumber = null,
        string? notes = null,
        Guid? createdByUserId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            VaultId = vaultId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
}
