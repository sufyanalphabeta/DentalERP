using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetDoctorPerformance;

public sealed class GetDoctorPerformanceQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetDoctorPerformanceQuery, Result<List<DoctorPerformanceDto>>>
{
    public async Task<Result<List<DoctorPerformanceDto>>> Handle(GetDoctorPerformanceQuery request, CancellationToken cancellationToken)
    {
        var fromDate = DateTime.UtcNow.AddMonths(-request.Months);

        // Revenue per doctor from invoices
        var invoiceRows = await db.Invoices
            .Where(i => i.Status != "Cancelled" && i.Status != "Draft" && i.CreatedAt >= fromDate)
            .Select(i => new { i.DoctorId, i.TotalAmount })
            .ToListAsync(cancellationToken);

        // Commission totals per doctor
        var commissionRows = await db.CommissionRecords
            .Where(c => c.CreatedAt >= fromDate)
            .Select(c => new { c.DoctorId, c.CommissionAmount, c.CommissionRate })
            .ToListAsync(cancellationToken);

        var doctorIds = invoiceRows.Select(r => r.DoctorId)
            .Union(commissionRows.Select(r => r.DoctorId))
            .Distinct().ToList();

        var doctorNames = await db.UserNames
            .Where(u => doctorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var revenueByDoctor = invoiceRows.GroupBy(r => r.DoctorId)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Total: g.Sum(r => r.TotalAmount)));

        var commissionByDoctor = commissionRows.GroupBy(r => r.DoctorId)
            .ToDictionary(g => g.Key, g => (
                Total: g.Sum(r => r.CommissionAmount),
                AvgRate: g.Average(r => r.CommissionRate)
            ));

        var dtos = doctorIds.Select(id =>
        {
            var rev = revenueByDoctor.GetValueOrDefault(id, (Count: 0, Total: 0m));
            var com = commissionByDoctor.GetValueOrDefault(id, (Total: 0m, AvgRate: 0m));
            return new DoctorPerformanceDto(
                id,
                doctorNames.GetValueOrDefault(id, "—"),
                rev.Count,
                rev.Total,
                com.Total,
                com.AvgRate
            );
        }).OrderByDescending(d => d.TotalRevenue).ToList();

        return Result.Success(dtos);
    }
}
