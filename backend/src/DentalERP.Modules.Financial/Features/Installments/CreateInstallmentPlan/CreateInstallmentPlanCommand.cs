using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Installments.CreateInstallmentPlan;

public sealed record CreateInstallmentPlanCommand(
    Guid InvoiceId,
    Guid PatientId,
    decimal TotalAmount,
    short InstallmentsCount,
    DateTime StartDate,
    string? Notes = null,
    Guid? CreatedByUserId = null) : IRequest<Result<Guid>>;
