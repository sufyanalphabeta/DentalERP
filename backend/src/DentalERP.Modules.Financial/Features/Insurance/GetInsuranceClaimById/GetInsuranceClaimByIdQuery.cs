using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.GetInsuranceClaimById;

public sealed record GetInsuranceClaimByIdQuery(Guid Id) : IRequest<Result<InsuranceClaimDetailDto>>;

public sealed record InsuranceClaimDetailDto(
    Guid Id,
    string ClaimNumber,
    string Status,
    Guid InsuranceCompanyId,
    string InsuranceCompanyName,
    Guid PatientId,
    string PatientName,
    Guid InvoiceId,
    decimal ClaimedAmount,
    decimal PaidAmount,
    decimal CoveragePercent,
    string? RejectionReason,
    string? Notes,
    DateTime ClaimDate,
    DateTime? SubmittedAt,
    List<InsurancePaymentDto> Payments
);

public sealed record InsurancePaymentDto(
    Guid Id,
    decimal Amount,
    string? ReferenceNumber,
    DateTime PaymentDate
);
