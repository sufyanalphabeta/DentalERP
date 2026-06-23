using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetExecutiveDashboard;

public sealed record GetExecutiveDashboardQuery : IRequest<Result<ExecutiveDashboardDto>>;

public sealed record RevenueSummaryDto(
    decimal InvoicedThisMonth,
    decimal CollectedThisMonth,
    decimal Outstanding,
    decimal CollectionRate);

public sealed record InsuranceSummaryDto(
    decimal SubmittedTotal,
    decimal PartiallyPaidBalance,
    decimal TotalOutstanding,
    int ClaimsThisMonth);

public sealed record OperationsSummaryDto(
    int AppointmentsThisMonth,
    int Attended,
    int NoShow,
    decimal UtilizationRate,
    int NewPatientsThisMonth);

public sealed record ExpenseTopCategory(string Category, decimal Amount);

public sealed record ExpenseSummaryDto(
    decimal ThisMonth,
    decimal LastMonth,
    double DeltaPct,
    List<ExpenseTopCategory> TopCategories);

public sealed record AssetAlertDto(
    int UnderMaintenance,
    int TotalActive);

public sealed record ExecutiveDashboardDto(
    RevenueSummaryDto Revenue,
    InsuranceSummaryDto Insurance,
    OperationsSummaryDto Operations,
    ExpenseSummaryDto Expenses,
    AssetAlertDto AssetAlerts);

public sealed class GetExecutiveDashboardQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetExecutiveDashboardQuery, Result<ExecutiveDashboardDto>>
{
    public async Task<Result<ExecutiveDashboardDto>> Handle(
        GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);
        var thisMonthStart = DateOnly.FromDateTime(startOfThisMonth);
        var thisMonthEnd = DateOnly.FromDateTime(now);
        var lastMonthStart = DateOnly.FromDateTime(startOfLastMonth);
        var lastMonthEnd = DateOnly.FromDateTime(startOfThisMonth.AddDays(-1));

        // ── Revenue ─────────────────────────────────────────────────
        var invoicesThisMonth = await db.Invoices
            .Where(i => i.Status != "Draft" && i.Status != "Cancelled" && i.CreatedAt >= startOfThisMonth)
            .Select(i => new { i.TotalAmount, i.PaidAmount, i.Status })
            .ToListAsync(cancellationToken);

        var invoicedThisMonth = invoicesThisMonth.Sum(i => i.TotalAmount);
        var outstanding = invoicesThisMonth
            .Where(i => i.Status == "Confirmed")
            .Sum(i => i.TotalAmount - i.PaidAmount);

        var collectedThisMonth = await db.Payments
            .Where(p => p.CreatedAt >= startOfThisMonth)
            .SumAsync(p => p.Amount, cancellationToken);

        var collectionRate = invoicedThisMonth > 0
            ? Math.Round(collectedThisMonth / invoicedThisMonth * 100, 1)
            : 0;

        // ── Insurance ───────────────────────────────────────────────
        var insuranceClaims = await db.InsuranceClaims
            .Select(c => new { c.Status, c.ClaimedAmount, c.PaidAmount, c.CreatedAt })
            .ToListAsync(cancellationToken);

        var submittedTotal = insuranceClaims
            .Where(c => c.Status == "Submitted")
            .Sum(c => c.ClaimedAmount);
        var partialBalance = insuranceClaims
            .Where(c => c.Status == "PartiallyPaid")
            .Sum(c => c.ClaimedAmount - c.PaidAmount);
        var claimsThisMonth = insuranceClaims
            .Count(c => c.CreatedAt >= startOfThisMonth);

        // ── Expenses (cross-module via raw SQL) ─────────────────────
        var expensesSql = await db.Database
            .SqlQuery<ExpenseRow>($"""
                SELECT
                  COALESCE(SUM(CASE WHEN expense_date >= {thisMonthStart} AND expense_date <= {thisMonthEnd} THEN amount ELSE 0 END), 0) AS "ThisMonth",
                  COALESCE(SUM(CASE WHEN expense_date >= {lastMonthStart} AND expense_date <= {lastMonthEnd} THEN amount ELSE 0 END), 0) AS "LastMonth"
                FROM expenses
                WHERE deleted_at IS NULL
                """)
            .FirstOrDefaultAsync(cancellationToken);

        var expThisMonth = expensesSql?.ThisMonth ?? 0m;
        var expLastMonth = expensesSql?.LastMonth ?? 0m;
        var expDelta = expLastMonth > 0
            ? (double)Math.Round((expThisMonth - expLastMonth) / expLastMonth * 100, 1)
            : 0;

        var topCategories = await db.Database
            .SqlQuery<ExpenseCategoryRow>($"""
                SELECT ec.name_ar AS "CategoryName", COALESCE(SUM(e.amount), 0) AS "Amount"
                FROM expenses e
                LEFT JOIN expense_categories ec ON ec.id = e.category_id
                WHERE e.deleted_at IS NULL
                  AND e.expense_date >= {thisMonthStart} AND e.expense_date <= {thisMonthEnd}
                GROUP BY ec.name_ar
                ORDER BY 2 DESC
                LIMIT 3
                """)
            .ToListAsync(cancellationToken);

        // ── Operations — appointments (cross-module via raw SQL) ────
        var opsData = await db.Database
            .SqlQuery<AppointmentsRow>($"""
                SELECT
                  COUNT(*) AS "Total",
                  COUNT(*) FILTER (WHERE status = 'Attended') AS "Attended",
                  COUNT(*) FILTER (WHERE status = 'NoShow') AS "NoShow"
                FROM appointments
                WHERE scheduled_at >= {startOfThisMonth} AND deleted_at IS NULL
                """)
            .FirstOrDefaultAsync(cancellationToken);

        var apptTotal = (int)(opsData?.Total ?? 0);
        var apptAttended = (int)(opsData?.Attended ?? 0);
        var apptNoShow = (int)(opsData?.NoShow ?? 0);
        var utilization = apptTotal > 0
            ? Math.Round((decimal)apptAttended / apptTotal * 100, 1)
            : 0;

        var newPatients = await db.Database
            .SqlQuery<CountRow>($"""
                SELECT COUNT(*) AS "Value"
                FROM patients
                WHERE created_at >= {startOfThisMonth} AND deleted_at IS NULL
                """)
            .Select(r => r.Value)
            .FirstOrDefaultAsync(cancellationToken);

        // ── Asset Alerts (cross-module via raw SQL) ─────────────────
        var assetData = await db.Database
            .SqlQuery<AssetRow>($"""
                SELECT
                  COUNT(*) FILTER (WHERE status = 'UnderMaintenance') AS "UnderMaintenance",
                  COUNT(*) FILTER (WHERE status = 'Active') AS "TotalActive"
                FROM assets
                WHERE deleted_at IS NULL
                """)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new ExecutiveDashboardDto(
            Revenue: new RevenueSummaryDto(invoicedThisMonth, collectedThisMonth, outstanding, collectionRate),
            Insurance: new InsuranceSummaryDto(submittedTotal, partialBalance, submittedTotal + partialBalance, claimsThisMonth),
            Operations: new OperationsSummaryDto(apptTotal, apptAttended, apptNoShow, utilization, (int)newPatients),
            Expenses: new ExpenseSummaryDto(expThisMonth, expLastMonth, expDelta,
                topCategories.Select(c => new ExpenseTopCategory(c.CategoryName ?? "غير مصنف", c.Amount)).ToList()),
            AssetAlerts: new AssetAlertDto(
                (int)(assetData?.UnderMaintenance ?? 0),
                (int)(assetData?.TotalActive ?? 0)));

        return Result.Success(dto);
    }

    private sealed record ExpenseRow(decimal ThisMonth, decimal LastMonth);
    private sealed record ExpenseCategoryRow(string? CategoryName, decimal Amount);
    private sealed record AppointmentsRow(long Total, long Attended, long NoShow);
    private sealed record CountRow(long Value);
    private sealed record AssetRow(long UnderMaintenance, long TotalActive);
}
