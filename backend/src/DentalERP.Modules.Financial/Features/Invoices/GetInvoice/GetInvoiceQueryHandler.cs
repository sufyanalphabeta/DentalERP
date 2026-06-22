using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Invoices.GetInvoice;

public sealed class GetInvoiceQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInvoiceQuery, Result<InvoiceDetailDto>>
{
    public async Task<Result<InvoiceDetailDto>> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure<InvoiceDetailDto>(new Error("Invoice.NotFound", "الفاتورة غير موجودة"));

        var payments = await db.Payments
            .Where(p => p.InvoiceId == request.InvoiceId)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PaymentDto(p.Id, p.Amount, p.PaymentMethod, p.ReferenceNumber, p.CreatedAt))
            .ToListAsync(cancellationToken);

        var patientName = await db.PatientNames
            .Where(p => p.Id == invoice.PatientId)
            .Select(p => p.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? "—";

        var doctorName = await db.UserNames
            .Where(u => u.Id == invoice.DoctorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? "—";

        var dto = new InvoiceDetailDto(
            invoice.Id,
            invoice.InvoiceNumber,
            patientName,
            doctorName,
            invoice.Status,
            invoice.Subtotal,
            invoice.DiscountTotal,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.Remaining,
            invoice.Currency,
            invoice.Notes,
            invoice.CancelledReason,
            invoice.CreatedAt,
            invoice.Items.Select(i => new InvoiceItemDto(
                i.Id, i.ServiceName, i.ServiceCode, i.Quantity,
                i.UnitPrice, i.Discount, i.Total, i.ProcedureId)).ToList(),
            payments);

        return Result.Success(dto);
    }
}
