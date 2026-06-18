using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.CreateInvoice;

public sealed record CreateInvoiceCommand(
    Guid PatientId,
    Guid DoctorId,
    List<InvoiceItemRequest> Items,
    string? Notes = null,
    Guid? CreatedByUserId = null) : IRequest<Result<Guid>>;

public sealed record InvoiceItemRequest(
    string ServiceName,
    decimal UnitPrice,
    short Quantity = 1,
    decimal Discount = 0,
    Guid? ProcedureId = null,
    string? ServiceCode = null);
