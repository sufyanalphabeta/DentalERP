using DentalERP.Modules.Clinical.Features.TreatmentPlans.CreateTreatmentPlan;
using DentalERP.Modules.Clinical.Features.TreatmentPlans.UpdateTreatmentPlanStatus;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class TreatmentPlanEndpoints
{
    public static IEndpointRouteBuilder MapTreatmentPlanEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/patients/{patientId}/treatment-plans",
            async (Guid patientId, CreateTreatmentPlanRequest req, ISender sender,
                ClaimsPrincipal user, CancellationToken ct) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await sender.Send(new CreateTreatmentPlanCommand(
                    patientId, userId, req.Title, req.EstimatedCost, req.Priority,
                    req.Description, req.Notes, req.StartDate, req.EndDate, req.Items), ct);
                return result.IsSuccess
                    ? Results.Created($"/api/treatment-plans/{result.Value}", new { id = result.Value })
                    : Results.BadRequest(result.Error);
            })
            .RequirePermission("Clinical.TreatmentPlans.Create")
            .WithName("CreateTreatmentPlan")
            .Produces(201).Produces(400).Produces(401);

        app.MapPatch("/api/treatment-plans/{id}/status",
            async (Guid id, UpdateStatusRequest req, ISender sender,
                ClaimsPrincipal user, CancellationToken ct) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await sender.Send(new UpdateTreatmentPlanStatusCommand(id, req.Status, userId), ct);
                return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
            })
            .RequireAnyPermission("Clinical.TreatmentPlans.Edit", "Clinical.TreatmentPlans.Approve")
            .WithName("UpdateTreatmentPlanStatus")
            .Produces(204).Produces(400).Produces(401);

        return app;
    }
}

public sealed record CreateTreatmentPlanRequest(
    string Title,
    decimal EstimatedCost,
    string Priority = "Normal",
    string? Description = null,
    string? Notes = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    List<CreateTreatmentPlanItemDto>? Items = null);

public sealed record UpdateStatusRequest(string Status);
