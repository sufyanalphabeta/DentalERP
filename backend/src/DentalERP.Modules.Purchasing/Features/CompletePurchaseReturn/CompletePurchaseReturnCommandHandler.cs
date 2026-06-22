using DentalERP.Modules.Purchasing.Domain.Internal;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CompletePurchaseReturn;

// RefundVaultId: optional — provide when supplier refunds cash.
// If null, the return is settled by credit note (no vault credit created).
public sealed record CompletePurchaseReturnCommand(
    Guid ReturnId,
    Guid? CompletedById,
    Guid? RefundVaultId = null) : IRequest<Result>;

public sealed class CompletePurchaseReturnCommandHandler(PurchasingDbContext db)
    : IRequestHandler<CompletePurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(CompletePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure(Error.NotFound("PurchaseReturn"));

        // Validate vault if a cash refund is being recorded
        if (request.RefundVaultId.HasValue)
        {
            var vaultStatus = await db.Database
                .SqlQuery<int?>($"SELECT CASE WHEN is_active THEN 1 ELSE 0 END AS \"Value\" FROM vaults WHERE id = {request.RefundVaultId.Value}")
                .FirstOrDefaultAsync(cancellationToken);

            if (vaultStatus is null)
                return Result.Failure(new Error("Vault.NotFound", "الخزينة المحددة غير موجودة."));
            if (vaultStatus == 0)
                return Result.Failure(new Error("Vault.Inactive", "الخزينة المحددة غير نشطة."));
        }

        var result = ret.Complete();
        if (result.IsFailure) return result;

        // Record vault credit if supplier paid cash refund
        if (request.RefundVaultId.HasValue)
        {
            db.VaultTransactions.Add(new VaultTransactionEntry
            {
                Id = Guid.NewGuid(),
                VaultId = request.RefundVaultId.Value,
                TransactionType = "supplier_refund",
                Amount = ret.TotalAmount,
                Direction = "in",
                Notes = $"Refund received for purchase return {ret.ReturnNumber}",
                CreatedByUserId = request.CompletedById,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "PurchaseReturn",
            EntityId   = ret.Id,
            Action     = "PurchaseReturn.Completed",
            PerformedById = request.CompletedById,
            Details    = request.RefundVaultId.HasValue
                ? $"Return {ret.ReturnNumber} completed. Vault refund recorded. Amount: {ret.TotalAmount}"
                : $"Return {ret.ReturnNumber} completed. Settled by credit note.",
            CreatedAt  = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
