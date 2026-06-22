using DentalERP.Modules.Financial.Features.AdvancePayments.CreateAdvancePayment;
using DentalERP.Modules.Financial.Features.Installments.CreateInstallmentPlan;
using DentalERP.Modules.Financial.Features.Installments.GetInstallmentPlans;
using DentalERP.Modules.Financial.Features.Installments.PayInstallment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class InstallmentEndpoints
{
    public static IEndpointRouteBuilder MapInstallmentEndpoints(this IEndpointRouteBuilder app)
    {
        var installments = app.MapGroup("/api/installments").RequireAuthorization();

        installments.MapGet("/plans", async (IMediator mediator, Guid? patientId) =>
        {
            var result = await mediator.Send(new GetInstallmentPlansQuery(patientId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        installments.MapPost("/plans", async (IMediator mediator, CreateInstallmentPlanCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/installments/plans/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        installments.MapPost("/{planId:guid}/pay/{num:int}", async (IMediator mediator, Guid planId, short num, PayInstallmentRequest req) =>
        {
            var result = await mediator.Send(new PayInstallmentCommand(planId, num, req.VaultId, req.PaymentMethod));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        var advance = app.MapGroup("/api/advance-payments").RequireAuthorization();
        advance.MapPost("/", async (IMediator mediator, CreateAdvancePaymentCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/advance-payments/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        return app;
    }

    private sealed record PayInstallmentRequest(Guid VaultId, string PaymentMethod);
}
