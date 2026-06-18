using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.GetInvoices;

public sealed record GetInvoicesQuery(
    Guid? PatientId = null,
    Guid? DoctorId = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<InvoicesPageDto>>;

public sealed record InvoicesPageDto(int TotalCount, List<InvoiceSummaryDto> Items);

public sealed record InvoiceSummaryDto(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    Guid DoctorId,
    string Status,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal Remaining,
    DateTime CreatedAt);
