using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaims;

public sealed record GetInsuranceClaimsQuery(
    Guid? PatientId,
    Guid? InsuranceCompanyId,
    string? Status,
    int Page,
    int PageSize
) : IRequest<Result<PagedInsuranceClaimsDto>>;

public sealed record PagedInsuranceClaimsDto(List<InsuranceClaimSummaryDto> Items, int TotalCount, int Page, int PageSize);

public sealed record InsuranceClaimSummaryDto(
    Guid Id,
    string ClaimNumber,
    string Status,
    string InsuranceCompanyName,
    Guid PatientId,
    string PatientName,
    decimal ClaimedAmount,
    decimal PaidAmount,
    decimal CoveragePercent,
    DateTime ClaimDate
);
