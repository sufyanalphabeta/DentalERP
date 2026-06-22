using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.RecordInsurancePayment;

public sealed class RecordInsurancePaymentCommandHandler(FinancialDbContext db)
    : IRequestHandler<RecordInsurancePaymentCommand, Result>
{
    public async Task<Result> Handle(RecordInsurancePaymentCommand request, CancellationToken cancellationToken)
    {
        var claim = await db.InsuranceClaims
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);

        if (claim is null)
            return Result.Failure(new Error("InsuranceClaim.NotFound", "Insurance claim not found."));

        var payment = InsurancePayment.Create(
            request.ClaimId, request.Amount,
            request.ReferenceNumber, request.Notes, request.ReceivedById);

        var result = claim.RecordPayment(payment);
        if (!result.IsSuccess) return result;

        if (request.VaultId.HasValue)
        {
            var vaultTx = VaultTransaction.Create(
                request.VaultId.Value,
                "general_receipt",
                request.Amount,
                "in",
                referenceNumber: request.ReferenceNumber,
                notes: request.Notes,
                createdByUserId: request.ReceivedById);
            db.VaultTransactions.Add(vaultTx);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
