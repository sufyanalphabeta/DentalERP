using DentalERP.Modules.Clinical.Features.Procedures.AddProcedure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class ProcedureEndpoints
{
    public static IEndpointRouteBuilder MapProcedureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/appointments/{appointmentId}/procedures",
            async (Guid appointmentId, AddProcedureRequest req, ISender sender,
                ClaimsPrincipal user, CancellationToken ct) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await sender.Send(new AddProcedureCommand(
                    appointmentId, req.PatientId, userId, req.ProcedureName,
                    req.ToothId, req.Surface, req.ProcedureCode, req.TreatmentPlanItemId,
                    req.ServiceId, req.Notes, req.DurationMinutes,
                    req.UpdateChartEntry, req.ChartCondition), ct);
                return result.IsSuccess
                    ? Results.Created($"/api/procedures/{result.Value}", new { id = result.Value })
                    : Results.BadRequest(result.Error);
            })
            .RequireAuthorization()
            .WithName("AddProcedure")
            .Produces(201).Produces(400).Produces(401);

        return app;
    }
}

public sealed record AddProcedureRequest(
    Guid PatientId,
    string ProcedureName,
    short? ToothId = null,
    string? Surface = null,
    string? ProcedureCode = null,
    Guid? TreatmentPlanItemId = null,
    Guid? ServiceId = null,
    string? Notes = null,
    int? DurationMinutes = null,
    bool UpdateChartEntry = false,
    string? ChartCondition = null);
