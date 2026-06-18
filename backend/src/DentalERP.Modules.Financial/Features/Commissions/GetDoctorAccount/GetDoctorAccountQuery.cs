using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Commissions.GetDoctorAccount;

public sealed record GetDoctorAccountQuery(
    Guid DoctorId,
    DateTime? From = null,
    DateTime? To = null) : IRequest<Result<DoctorAccountDto>>;

public sealed record DoctorAccountDto(
    Guid DoctorId,
    decimal TotalCommissionDue,
    decimal TotalCommissionPaid,
    decimal RemainingCommission,
    List<CommissionLineDto> Records);

public sealed record CommissionLineDto(
    Guid Id,
    Guid InvoiceId,
    string CommissionMethod,
    decimal BaseAmount,
    decimal CommissionRate,
    decimal CommissionAmount,
    bool IsPaid,
    DateTime? PaidAt,
    DateTime CreatedAt);
