using DentalERP.Modules.Clinical.Features.Chart.GetChart;
using DentalERP.Modules.Clinical.Features.Chart.UpdateChart;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class ChartEndpoints
{
    public static IEndpointRouteBuilder MapChartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients/{patientId}/chart")
            .RequireAuthorization();

        group.MapGet("", async (Guid patientId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetChartQuery(patientId), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .WithName("GetChart")
        .Produces<GetChartResponse>()
        .Produces(401).Produces(404);

        group.MapPost("", async (Guid patientId, UpdateChartRequest req, ISender sender,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new UpdateChartCommand(
                patientId, req.ToothId, req.Condition, userId,
                req.Surface, req.Severity, req.Notes, req.AppointmentId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/patients/{patientId}/chart/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("UpdateChart")
        .Produces(201).Produces(400).Produces(401);

        return app;
    }
}

public sealed record UpdateChartRequest(
    short ToothId,
    string Condition,
    string? Surface = null,
    string? Severity = null,
    string? Notes = null,
    Guid? AppointmentId = null);
