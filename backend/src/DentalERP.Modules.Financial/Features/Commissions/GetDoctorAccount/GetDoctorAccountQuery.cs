using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Commissions.GetDoctorAccount;

public sealed record GetDoctorAccountQuery(
    Guid DoctorId,
    DateTime? From = null,
    DateTime? To = null) : IRequest<Result<DoctorAccountDto>>;

public sealed record DoctorAccountDto(
    Guid DoctorId,
    string DoctorName,
    string CommissionMethod,
    decimal DefaultCommissionValue,
    decimal TotalUnpaid,
    decimal TotalPaid,
    List<CommissionLineDto> Commissions);

public sealed record CommissionLineDto(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    Guid PaymentId,
    string CommissionMethod,
    decimal BaseAmount,
    decimal CommissionRate,
    decimal CommissionAmount,
    bool IsPaid,
    DateTime? PaidAt,
    DateTime CreatedAt);
