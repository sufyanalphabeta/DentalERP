namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InsurancePayment
{
    public Guid Id { get; private set; }
    public Guid InsuranceClaimId { get; private set; }
    public decimal Amount { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public Guid ReceivedById { get; private set; }

    public InsuranceClaim InsuranceClaim { get; private set; } = default!;

    private InsurancePayment() { }

    public static InsurancePayment Create(Guid claimId, decimal amount,
        string? referenceNumber, string? notes, Guid receivedById)
    {
        return new InsurancePayment
        {
            Id = Guid.NewGuid(),
            InsuranceClaimId = claimId,
            Amount = amount,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            PaymentDate = DateTime.UtcNow,
            ReceivedById = receivedById
        };
    }
}
