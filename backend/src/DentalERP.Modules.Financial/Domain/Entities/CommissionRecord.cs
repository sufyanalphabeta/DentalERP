namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class CommissionRecord
{
    public Guid Id { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid PaymentId { get; private set; }
    public Guid? ProcedureId { get; private set; }
    public string CommissionMethod { get; private set; } = string.Empty;
    public decimal BaseAmount { get; private set; }
    public decimal CommissionRate { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Guid? VaultTransactionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CommissionRecord() { }

    public static CommissionRecord Create(
        Guid doctorId,
        Guid invoiceId,
        Guid paymentId,
        string commissionMethod,
        decimal baseAmount,
        decimal commissionRate,
        decimal commissionAmount,
        Guid? procedureId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            InvoiceId = invoiceId,
            PaymentId = paymentId,
            ProcedureId = procedureId,
            CommissionMethod = commissionMethod,
            BaseAmount = baseAmount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow
        };

    public void MarkPaid(Guid vaultTransactionId)
    {
        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        VaultTransactionId = vaultTransactionId;
    }
}
