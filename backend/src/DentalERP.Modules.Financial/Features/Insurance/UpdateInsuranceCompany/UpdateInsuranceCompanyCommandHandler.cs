using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.UpdateInsuranceCompany;

public sealed class UpdateInsuranceCompanyCommandHandler(FinancialDbContext db)
    : IRequestHandler<UpdateInsuranceCompanyCommand, Result>
{
    public async Task<Result> Handle(UpdateInsuranceCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await db.InsuranceCompanies.FindAsync([request.Id], cancellationToken);
        if (company is null)
            return Result.Failure(new Error("InsuranceCompany.NotFound", "شركة التأمين غير موجودة"));

        company.Update(request.Name, request.NameAr, request.ContactPerson,
            request.Phone, request.Email, request.DefaultCoveragePercent);

        if (request.IsActive) company.Activate();
        else company.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
