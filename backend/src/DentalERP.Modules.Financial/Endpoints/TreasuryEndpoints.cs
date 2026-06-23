using System.Security.Claims;
using DentalERP.Modules.Financial.Features.Treasury.GetVaultBalances;
using DentalERP.Modules.Financial.Features.Treasury.GetVaultMovements;
using DentalERP.Modules.Financial.Features.Treasury.GetVaultTransfers;
using DentalERP.Modules.Financial.Features.Vaults.CreateVault;
using DentalERP.Modules.Financial.Features.Vaults.CreateVaultTransfer;
using DentalERP.Modules.Financial.Features.Vaults.UpdateVault;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public sealed record UpdateVaultRequest(string Name, string Type);
public sealed record CreateTransferRequest(Guid FromVaultId, Guid ToVaultId, decimal Amount, string? Notes);

public static class TreasuryEndpoints
{
    public static IEndpointRouteBuilder MapTreasuryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/treasury");

        group.MapGet("/vaults/balances", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetVaultBalancesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.View");

        group.MapPost("/vaults", async (IMediator mediator, CreateVaultCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess
                ? Results.Created($"/api/treasury/vaults/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.Create");

        group.MapPut("/vaults/{id:guid}", async (IMediator mediator, Guid id, UpdateVaultRequest req) =>
        {
            var result = await mediator.Send(new UpdateVaultCommand(id, req.Name, req.Type));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.Edit");

        group.MapPost("/transfers", async (IMediator mediator, ClaimsPrincipal user, CreateTransferRequest req) =>
        {
            var userId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid)
                ? uid
                : Guid.Empty;
            var result = await mediator.Send(new CreateVaultTransferCommand(
                req.FromVaultId, req.ToVaultId, req.Amount, req.Notes, userId));
            return result.IsSuccess
                ? Results.Created($"/api/treasury/transfers/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.Transfer");

        group.MapGet("/transfers", async (IMediator mediator, int page = 1, int pageSize = 30) =>
        {
            var result = await mediator.Send(new GetVaultTransfersQuery(page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.View");

        group.MapGet("/movements", async (
            IMediator mediator,
            Guid? vaultId,
            string? direction,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1,
            int pageSize = 30) =>
        {
            var result = await mediator.Send(new GetVaultMovementsQuery(
                vaultId, direction, dateFrom, dateTo, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Treasury.View");

        return app;
    }
}
