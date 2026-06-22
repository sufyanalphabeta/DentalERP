using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.ConfirmInvoice;

public sealed record ConfirmInvoiceCommand(Guid InvoiceId) : IRequest<Result>;
