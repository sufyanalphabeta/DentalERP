using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.UpdateInsuranceCompany;

public sealed record UpdateInsuranceCompanyCommand(
    Guid Id,
    string Name,
    string? NameAr,
    string? ContactPerson,
    string? Phone,
    string? Email,
    decimal DefaultCoveragePercent,
    bool IsActive) : IRequest<Result>;
