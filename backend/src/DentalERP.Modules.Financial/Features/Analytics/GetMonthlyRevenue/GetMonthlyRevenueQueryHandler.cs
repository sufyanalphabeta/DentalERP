using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetMonthlyRevenue;

public sealed class GetMonthlyRevenueQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetMonthlyRevenueQuery, Result<List<MonthlyRevenueDto>>>
{
    private static readonly string[] ArabicMonths =
        ["يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"];

    public async Task<Result<List<MonthlyRevenueDto>>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
    {
        var fromDate = DateTime.UtcNow.AddMonths(-request.Months + 1);
        var startOfMonth = new DateTime(fromDate.Year, fromDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var invoices = await db.Invoices
            .Where(i => i.Status != "Cancelled" && i.Status != "Draft" && i.CreatedAt >= startOfMonth)
            .Select(i => new { i.CreatedAt, i.TotalAmount, i.PaidAmount })
            .ToListAsync(cancellationToken);

        var grouped = invoices
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto(
                g.Key.Year,
                g.Key.Month,
                $"{ArabicMonths[g.Key.Month - 1]} {g.Key.Year}",
                g.Sum(i => i.TotalAmount),
                g.Sum(i => i.PaidAmount),
                g.Sum(i => i.TotalAmount - i.PaidAmount),
                g.Count()
            ))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        // Fill in months with zero data
        var result = new List<MonthlyRevenueDto>();
        for (int i = 0; i < request.Months; i++)
        {
            var date = startOfMonth.AddMonths(i);
            var existing = grouped.FirstOrDefault(m => m.Year == date.Year && m.Month == date.Month);
            result.Add(existing ?? new MonthlyRevenueDto(
                date.Year, date.Month,
                $"{ArabicMonths[date.Month - 1]} {date.Year}",
                0, 0, 0, 0));
        }

        return Result.Success(result);
    }
}
