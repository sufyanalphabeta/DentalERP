using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Payments.AddPayment;

public sealed record AddPaymentCommand(
    Guid InvoiceId,
    Guid VaultId,
    decimal Amount,
    string PaymentMethod,
    string? ReferenceNumber = null,
    string? Notes = null,
    Guid? CreatedByUserId = null) : IRequest<Result<Guid>>;
