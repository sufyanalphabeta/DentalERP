using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.AdvancePayments.CreateAdvancePayment;

public sealed record CreateAdvancePaymentCommand(
    Guid PatientId,
    Guid VaultId,
    decimal Amount,
    string? Notes = null,
    Guid? CreatedByUserId = null) : IRequest<Result<Guid>>;
