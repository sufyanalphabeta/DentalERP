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
            query = query.Where(i => i.CreatedAt >= DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc));
        if (request.To.HasValue)
            query = query.Where(i => i.CreatedAt <= DateTime.SpecifyKind(request.To.Value.AddDays(1), DateTimeKind.Utc));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            var matchingPatientIds = await db.PatientNames
                .Where(p => p.FullName.Contains(s))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            query = query.Where(i => i.InvoiceNumber.Contains(s) || matchingPatientIds.Contains(i.PatientId));
        }

        var total = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new { i.Id, i.InvoiceNumber, i.PatientId, i.DoctorId, i.Status, i.TotalAmount, i.PaidAmount, i.Currency, i.CreatedAt })
            .ToListAsync(cancellationToken);

        var patientIds = invoices.Select(i => i.PatientId).Distinct().ToList();
        var doctorIds = invoices.Select(i => i.DoctorId).Distinct().ToList();

        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var doctorNames = await db.UserNames
            .Where(u => doctorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var items = invoices.Select(i => new InvoiceSummaryDto(
            i.Id,
            i.InvoiceNumber,
            patientNames.GetValueOrDefault(i.PatientId, "—"),
            doctorNames.GetValueOrDefault(i.DoctorId, "—"),
            i.Status,
            i.TotalAmount,
            i.PaidAmount,
            i.TotalAmount - i.PaidAmount,
            i.Currency,
            i.CreatedAt)).ToList();

        return Result.Success(new InvoicesPageDto(total, request.Page, request.PageSize, items));
    }
}
