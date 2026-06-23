using DentalERP.Modules.Financial.Features.Analytics.GetARAgingPdf;
using DentalERP.Modules.Financial.Features.Analytics.GetARAgingReport;
using DentalERP.Modules.Financial.Features.Analytics.GetCollectionSummary;
using DentalERP.Modules.Financial.Features.Analytics.GetCollectionSummaryPdf;
using DentalERP.Modules.Financial.Features.Analytics.GetDoctorPerformance;
using DentalERP.Modules.Financial.Features.Analytics.GetExecutiveDashboard;
using DentalERP.Modules.Financial.Features.Analytics.GetInactivePatients;
using DentalERP.Modules.Financial.Features.Analytics.GetMonthlyRevenue;
using DentalERP.Modules.Financial.Features.Analytics.GetOverdueInstallments;
using DentalERP.Modules.Financial.Features.Analytics.GetPatientBalances;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var analytics = app.MapGroup("/api/analytics").RequireAuthorization();

        analytics.MapGet("/inactive-patients", async (IMediator mediator, int months = 6) =>
        {
            var result = await mediator.Send(new GetInactivePatientsQuery(months));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/overdue-installments", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOverdueInstallmentsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/monthly-revenue", async (IMediator mediator, int months = 6) =>
        {
            var result = await mediator.Send(new GetMonthlyRevenueQuery(months));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/doctor-performance", async (IMediator mediator, int months = 3) =>
        {
            var result = await mediator.Send(new GetDoctorPerformanceQuery(months));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/patient-balances", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPatientBalancesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/ar-aging", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetARAgingReportQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/ar-aging/pdf", async (IMediator mediator, string? clinicName) =>
        {
            var result = await mediator.Send(new GetARAgingPdfQuery(clinicName ?? "عيادة الأسنان"));
            return result.IsSuccess
                ? Results.File(result.Value, "application/pdf", "ar-aging.pdf")
                : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/collection-summary", async (IMediator mediator, DateOnly? from, DateOnly? to) =>
        {
            var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await mediator.Send(new GetCollectionSummaryQuery(fromDate, toDate));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/collection-summary/pdf", async (IMediator mediator, DateOnly? from, DateOnly? to, string? clinicName) =>
        {
            var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await mediator.Send(new GetCollectionSummaryPdfQuery(fromDate, toDate, clinicName ?? "عيادة الأسنان"));
            return result.IsSuccess
                ? Results.File(result.Value, "application/pdf", "collection-summary.pdf")
                : Results.BadRequest(result.Error);
        });

        analytics.MapGet("/executive-dashboard", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExecutiveDashboardQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        return app;
    }
}
