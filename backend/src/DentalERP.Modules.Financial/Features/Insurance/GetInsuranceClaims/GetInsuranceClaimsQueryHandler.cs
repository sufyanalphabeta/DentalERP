using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaims;

public sealed class GetInsuranceClaimsQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInsuranceClaimsQuery, Result<PagedInsuranceClaimsDto>>
{
    public async Task<Result<PagedInsuranceClaimsDto>> Handle(GetInsuranceClaimsQuery request, CancellationToken cancellationToken)
    {
        var query = db.InsuranceClaims
            .Include(c => c.InsuranceCompany)
            .AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(c => c.PatientId == request.PatientId);
        if (request.InsuranceCompanyId.HasValue) query = query.Where(c => c.InsuranceCompanyId == request.InsuranceCompanyId);
        if (!string.IsNullOrEmpty(request.Status)) query = query.Where(c => c.Status == request.Status);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(c => c.ClaimDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new { c.Id, c.ClaimNumber, c.Status, c.InsuranceCompany.Name, c.PatientId, c.ClaimedAmount, c.PaidAmount, c.CoveragePercent, c.ClaimDate })
            .ToListAsync(cancellationToken);

        var patientIds = rows.Select(r => r.PatientId).Distinct().ToList();
        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var items = rows.Select(c => new InsuranceClaimSummaryDto(
            c.Id, c.ClaimNumber, c.Status, c.Name,
            c.PatientId, patientNames.GetValueOrDefault(c.PatientId, "—"),
            c.ClaimedAmount, c.PaidAmount, c.CoveragePercent, c.ClaimDate)).ToList();

        return Result.Success(new PagedInsuranceClaimsDto(items, total, request.Page, request.PageSize));
    }
}
