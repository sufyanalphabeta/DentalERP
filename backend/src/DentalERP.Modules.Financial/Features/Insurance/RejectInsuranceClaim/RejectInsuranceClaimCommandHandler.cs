using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.RejectInsuranceClaim;

public sealed class RejectInsuranceClaimCommandHandler(FinancialDbContext db)
    : IRequestHandler<RejectInsuranceClaimCommand, Result>
{
    public async Task<Result> Handle(RejectInsuranceClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await db.InsuranceClaims.FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);
        if (claim is null)
            return Result.Failure(new Error("InsuranceClaim.NotFound", "Insurance claim not found."));

        var result = claim.Reject(request.Reason);
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
