using DentalERP.Modules.Financial.Features.Commissions.GetDoctorAccount;
using DentalERP.Modules.Financial.Features.Commissions.PayCommission;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class CommissionEndpoints
{
    public static IEndpointRouteBuilder MapCommissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/treasury").RequireAuthorization();

        group.MapGet("/doctors/{doctorId:guid}/account", async (IMediator mediator, Guid doctorId, DateTime? from, DateTime? to) =>
        {
            var result = await mediator.Send(new GetDoctorAccountQuery(doctorId, from, to));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/commissions/{commissionId:guid}/pay", async (IMediator mediator, Guid commissionId, PayCommissionRequest req) =>
        {
            var result = await mediator.Send(new PayCommissionCommand(commissionId, req.VaultId, req.PaidByUserId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        return app;
    }

    private sealed record PayCommissionRequest(Guid VaultId, Guid? PaidByUserId);
}
