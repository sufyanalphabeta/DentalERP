using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.GetInvoice;

public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<Result<InvoiceDetailDto>>;

public sealed record InvoiceDetailDto(
    Guid Id,
    string InvoiceNumber,
    string PatientName,
    string DoctorName,
    string Status,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal Remaining,
    string Currency,
    string? Notes,
    string? CancelledReason,
    DateTime CreatedAt,
    List<InvoiceItemDto> Items,
    List<PaymentDto> Payments);

public sealed record InvoiceItemDto(
    Guid Id,
    string ServiceName,
    string? ServiceCode,
    short Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total,
    Guid? ProcedureId);

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    string PaymentMethod,
    string? ReferenceNumber,
    DateTime CreatedAt);
