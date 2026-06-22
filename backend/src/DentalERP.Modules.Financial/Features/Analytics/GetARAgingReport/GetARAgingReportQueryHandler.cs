using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetARAgingReport;

public sealed record GetARAgingReportQuery() : IRequest<Result<ARAgingReportDto>>;

public sealed record ARAgingReportDto(
    DateTime AsOf,
    decimal TotalOutstanding,
    IReadOnlyList<ARAgingPatientDto> Patients);

public sealed record ARAgingPatientDto(
    Guid PatientId,
    string PatientName,
    decimal Current,
    decimal Days30,
    decimal Days60,
    decimal Days90,
    decimal Over90,
    decimal Total);

internal sealed class GetARAgingReportQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetARAgingReportQuery, Result<ARAgingReportDto>>
{
    public async Task<Result<ARAgingReportDto>> Handle(GetARAgingReportQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var outstandingInvoices = await db.Invoices
            .Where(i => i.Status != "Cancelled" && i.Status != "Draft" && i.TotalAmount > i.PaidAmount)
            .Select(i => new { i.PatientId, i.CreatedAt, Outstanding = i.TotalAmount - i.PaidAmount })
            .ToListAsync(ct);

        var patientIds = outstandingInvoices.Select(i => i.PatientId).Distinct().ToList();
        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, ct);

        var grouped = outstandingInvoices
            .GroupBy(i => i.PatientId)
            .Select(g =>
            {
                decimal current = 0, days30 = 0, days60 = 0, days90 = 0, over90 = 0;
                foreach (var inv in g)
                {
                    var age = (now - inv.CreatedAt).TotalDays;
                    if (age <= 30) current += inv.Outstanding;
                    else if (age <= 60) days30 += inv.Outstanding;
                    else if (age <= 90) days60 += inv.Outstanding;
                    else if (age <= 120) days90 += inv.Outstanding;
                    else over90 += inv.Outstanding;
                }
                return new ARAgingPatientDto(
                    g.Key,
                    patientNames.GetValueOrDefault(g.Key, "—"),
                    current, days30, days60, days90, over90,
                    current + days30 + days60 + days90 + over90);
            })
            .OrderByDescending(p => p.Total)
            .ToList();

        var total = grouped.Sum(p => p.Total);

        return Result.Success(new ARAgingReportDto(now, total, grouped));
    }
}
