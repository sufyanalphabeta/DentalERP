using DentalERP.Modules.Financial.Features.Analytics.GetDoctorPerformance;
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

        return app;
    }
}
