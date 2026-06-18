using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.Modules.Financial.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceClaim;

public sealed class CreateInsuranceClaimCommandHandler(FinancialDbContext db, IInsuranceClaimNumberGenerator claimNumberGen)
    : IRequestHandler<CreateInsuranceClaimCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInsuranceClaimCommand request, CancellationToken cancellationToken)
    {
        var existingClaim = await db.InsuranceClaims.AnyAsync(c => c.InvoiceId == request.InvoiceId, cancellationToken);
        if (existingClaim)
            return Result.Failure<Guid>(new Error("InsuranceClaim.Duplicate", "An insurance claim already exists for this invoice."));

        var companyExists = await db.InsuranceCompanies.AnyAsync(c => c.Id == request.InsuranceCompanyId && c.IsActive, cancellationToken);
        if (!companyExists)
            return Result.Failure<Guid>(new Error("InsuranceCompany.NotFound", "Insurance company not found or inactive."));

        var claimNumber = await claimNumberGen.GenerateAsync(cancellationToken);
        var claim = InsuranceClaim.Create(
            claimNumber, request.InvoiceId, request.InsuranceCompanyId,
            request.PatientId, request.ClaimedAmount, request.CoveragePercent, request.Notes);

        db.InsuranceClaims.Add(claim);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(claim.Id);
    }
}
