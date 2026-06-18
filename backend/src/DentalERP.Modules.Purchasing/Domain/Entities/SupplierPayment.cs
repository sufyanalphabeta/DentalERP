namespace DentalERP.Modules.Purchasing.Domain.Entities;

public sealed class SupplierPayment
{
    public Guid Id { get; private set; }
    public string PaymentNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid VaultId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? PaidById { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SupplierPayment() { }

    public static SupplierPayment Create(string paymentNumber, Guid supplierId, Guid vaultId,
        decimal amount, DateOnly paymentDate, string? referenceNumber = null, string? notes = null, Guid? paidById = null)
    {
        if (amount <= 0) throw new ArgumentException("Payment amount must be greater than zero.");

        return new SupplierPayment
        {
            Id = Guid.NewGuid(),
            PaymentNumber = paymentNumber,
            SupplierId = supplierId,
            VaultId = vaultId,
            Amount = amount,
            PaymentDate = paymentDate,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            PaidById = paidById,
            CreatedAt = DateTime.UtcNow
        };
    }
}
