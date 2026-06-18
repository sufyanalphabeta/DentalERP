using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.SubmitInsuranceClaim;

public sealed class SubmitInsuranceClaimCommandHandler(FinancialDbContext db)
    : IRequestHandler<SubmitInsuranceClaimCommand, Result>
{
    public async Task<Result> Handle(SubmitInsuranceClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await db.InsuranceClaims.FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);
        if (claim is null)
            return Result.Failure(new Error("InsuranceClaim.NotFound", "Insurance claim not found."));

        var result = claim.Submit();
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
