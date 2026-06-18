using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class AdvancePayment
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid VaultId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Remaining { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AdvancePayment() { }

    public static AdvancePayment Create(
        Guid patientId,
        Guid vaultId,
        decimal amount,
        string? notes = null,
        Guid? createdByUserId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            VaultId = vaultId,
            Amount = amount,
            Remaining = amount,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

    public Result Apply(decimal amount)
    {
        if (amount <= 0)
            return Result.Failure(new Error("AdvancePayment.InvalidAmount", "مبلغ التطبيق يجب أن يكون أكبر من صفر"));
        if (amount > Remaining)
            return Result.Failure(new Error("AdvancePayment.Insufficient", "الرصيد المتاح أقل من المبلغ المطلوب"));
        Remaining -= amount;
        return Result.Success();
    }
}
