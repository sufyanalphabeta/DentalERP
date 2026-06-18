using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceCompany;

public sealed class CreateInsuranceCompanyCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateInsuranceCompanyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInsuranceCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = InsuranceCompany.Create(
            request.Name, request.NameAr, request.ContactPerson,
            request.Phone, request.Email, request.DefaultCoveragePercent);

        db.InsuranceCompanies.Add(company);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(company.Id);
    }
}
