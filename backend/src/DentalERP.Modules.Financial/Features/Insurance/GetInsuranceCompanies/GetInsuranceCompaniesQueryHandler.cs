using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceCompanies;

public sealed class GetInsuranceCompaniesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInsuranceCompaniesQuery, Result<List<InsuranceCompanyDto>>>
{
    public async Task<Result<List<InsuranceCompanyDto>>> Handle(GetInsuranceCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = db.InsuranceCompanies.AsQueryable();
        if (request.ActiveOnly) query = query.Where(c => c.IsActive);

        var companies = await query
            .OrderBy(c => c.Name)
            .Select(c => new InsuranceCompanyDto(c.Id, c.Name, c.NameAr, c.Phone, c.DefaultCoveragePercent, c.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(companies);
    }
}
