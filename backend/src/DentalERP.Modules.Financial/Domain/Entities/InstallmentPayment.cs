using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InstallmentPayment
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public short InstallmentNum { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = "Pending"; // Pending|Paid|Overdue
    public DateTime? PaidAt { get; private set; }
    public Guid? VaultId { get; private set; }
    public string? PaymentMethod { get; private set; }

    private InstallmentPayment() { }

    internal static InstallmentPayment Create(Guid planId, short installmentNum, DateTime dueDate, decimal amount)
        => new()
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            InstallmentNum = installmentNum,
            DueDate = DateOnly.FromDateTime(dueDate),
            Amount = amount,
            Status = "Pending"
        };

    public Result Pay(Guid vaultId, string paymentMethod)
    {
        if (Status == "Paid")
            return Result.Failure(new Error("Installment.AlreadyPaid", "هذا القسط مدفوع بالفعل"));
        Status = "Paid";
        PaidAt = DateTime.UtcNow;
        VaultId = vaultId;
        PaymentMethod = paymentMethod;
        return Result.Success();
    }

    public void MarkOverdue()
    {
        if (Status == "Pending" && DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
            Status = "Overdue";
    }
}
