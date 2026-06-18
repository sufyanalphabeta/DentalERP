using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.AdvancePayments.CreateAdvancePayment;

public sealed class CreateAdvancePaymentCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateAdvancePaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAdvancePaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Result.Failure<Guid>(new Error("AdvancePayment.InvalidAmount", "مبلغ الدفعة المقدمة يجب أن يكون أكبر من صفر"));

        var vault = await db.Vaults.FindAsync([request.VaultId], cancellationToken);
        if (vault is null || !vault.IsActive)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "الخزينة غير موجودة أو غير نشطة"));

        var advance = AdvancePayment.Create(
            request.PatientId,
            request.VaultId,
            request.Amount,
            request.Notes,
            request.CreatedByUserId);

        var vaultTx = VaultTransaction.Create(
            request.VaultId,
            "receipt_from_patient",
            request.Amount,
            "in",
            relatedPatientId: request.PatientId,
            notes: $"دفعة مقدمة - {request.Notes}",
            createdByUserId: request.CreatedByUserId);

        db.AdvancePayments.Add(advance);
        db.VaultTransactions.Add(vaultTx);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(advance.Id);
    }
}
