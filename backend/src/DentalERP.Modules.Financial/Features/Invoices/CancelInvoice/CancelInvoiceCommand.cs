using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.CancelInvoice;

public sealed record CancelInvoiceCommand(Guid InvoiceId, string Reason) : IRequest<Result>;
