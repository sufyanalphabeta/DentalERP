using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceCompanies;

public sealed record GetInsuranceCompaniesQuery(bool ActiveOnly = true) : IRequest<Result<List<InsuranceCompanyDto>>>;

public sealed record InsuranceCompanyDto(Guid Id, string Name, string? NameAr, string? Phone, decimal DefaultCoveragePercent, bool IsActive);
