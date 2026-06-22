using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Analytics.GetOverdueInstallments;

public sealed record GetOverdueInstallmentsQuery() : IRequest<Result<List<OverdueInstallmentDto>>>;

public sealed record OverdueInstallmentDto(
    Guid InstallmentPlanId,
    string InvoiceNumber,
    Guid PatientId,
    string PatientName,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    int OverduePayments,
    DateOnly OldestDueDate
);
