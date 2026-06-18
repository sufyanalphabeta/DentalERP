using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class InstallmentPlan
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid PatientId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public short InstallmentsCount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<InstallmentPayment> _installments = [];
    public IReadOnlyList<InstallmentPayment> Installments => _installments;

    private InstallmentPlan() { }

    public static InstallmentPlan Create(
        Guid invoiceId,
        Guid patientId,
        decimal totalAmount,
        short installmentsCount,
        DateTime startDate,
        string? notes = null,
        Guid? createdByUserId = null)
    {
        var plan = new InstallmentPlan
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            PatientId = patientId,
            TotalAmount = totalAmount,
            InstallmentsCount = installmentsCount,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        var installmentAmount = Math.Round(totalAmount / installmentsCount, 2);
        for (short i = 1; i <= installmentsCount; i++)
        {
            plan._installments.Add(InstallmentPayment.Create(plan.Id, i, startDate.AddMonths(i - 1), installmentAmount));
        }
        return plan;
    }
}
