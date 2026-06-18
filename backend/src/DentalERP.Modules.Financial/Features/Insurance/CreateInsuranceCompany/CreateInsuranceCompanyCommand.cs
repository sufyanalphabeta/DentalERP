using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceCompany;

public sealed record CreateInsuranceCompanyCommand(
    string Name,
    string? NameAr,
    string? ContactPerson,
    string? Phone,
    string? Email,
    decimal DefaultCoveragePercent
) : IRequest<Result<Guid>>;
