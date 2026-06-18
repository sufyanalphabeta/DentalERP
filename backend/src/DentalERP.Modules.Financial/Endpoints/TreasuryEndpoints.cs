using DentalERP.Modules.Financial.Features.Treasury.GetVaultBalances;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class TreasuryEndpoints
{
    public static IEndpointRouteBuilder MapTreasuryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/treasury").RequireAuthorization();

        group.MapGet("/vaults/balances", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetVaultBalancesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        return app;
    }
}
