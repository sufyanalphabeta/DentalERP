using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Commissions.PayCommission;

public sealed class PayCommissionCommandHandler(FinancialDbContext db)
    : IRequestHandler<PayCommissionCommand, Result>
{
    public async Task<Result> Handle(PayCommissionCommand request, CancellationToken cancellationToken)
    {
        var commission = await db.CommissionRecords.FindAsync([request.CommissionId], cancellationToken);
        if (commission is null)
            return Result.Failure(new Error("Commission.NotFound", "سجل العمولة غير موجود"));
        if (commission.IsPaid)
            return Result.Failure(new Error("Commission.AlreadyPaid", "هذه العمولة مدفوعة بالفعل"));

        var vault = await db.Vaults.FindAsync([request.VaultId], cancellationToken);
        if (vault is null || !vault.IsActive)
            return Result.Failure(new Error("Vault.NotFound", "الخزينة غير موجودة أو غير نشطة"));

        var vaultTx = VaultTransaction.Create(
            request.VaultId,
            "payment_to_doctor",
            commission.CommissionAmount,
            "out",
            relatedDoctorId: commission.DoctorId,
            notes: $"دفع عمولة للطبيب",
            createdByUserId: request.PaidByUserId);

        db.VaultTransactions.Add(vaultTx);
        await db.SaveChangesAsync(cancellationToken);

        commission.MarkPaid(vaultTx.Id);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
