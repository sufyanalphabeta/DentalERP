using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Installments.GetInstallmentPlans;

public sealed record GetInstallmentPlansQuery(Guid? PatientId = null) : IRequest<Result<List<InstallmentPlanDto>>>;

public sealed record InstallmentPlanDto(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    string PatientName,
    decimal TotalAmount,
    int InstallmentsCount,
    DateTime CreatedAt,
    List<InstallmentPaymentDto> Installments);

public sealed record InstallmentPaymentDto(
    Guid Id,
    short InstallmentNum,
    string DueDate,
    decimal Amount,
    string Status,
    DateTime? PaidAt,
    string? PaymentMethod);
