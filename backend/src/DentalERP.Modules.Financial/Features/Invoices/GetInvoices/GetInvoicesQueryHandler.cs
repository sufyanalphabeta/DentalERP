using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Invoices.GetInvoices;

public sealed class GetInvoicesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInvoicesQuery, Result<InvoicesPageDto>>
{
    public async Task<Result<InvoicesPageDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Invoices.AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(i => i.PatientId == request.PatientId.Value);
        if (request.DoctorId.HasValue)
            query = query.Where(i => i.DoctorId == request.DoctorId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(i => i.Status == request.Status);
        if (request.From.HasValue)
            query = query.Where(i => i.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(i => i.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InvoiceSummaryDto(
                i.Id, i.InvoiceNumber, i.PatientId, i.DoctorId,
                i.Status, i.TotalAmount, i.PaidAmount,
                i.TotalAmount - i.PaidAmount, i.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new InvoicesPageDto(total, items));
    }
}
