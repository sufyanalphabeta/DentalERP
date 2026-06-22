using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.RecordInsurancePayment;

public sealed record RecordInsurancePaymentCommand(
    Guid ClaimId,
    decimal Amount,
    string? ReferenceNumber,
    string? Notes,
    Guid ReceivedById,
    Guid? VaultId = null
) : IRequest<Result>;
