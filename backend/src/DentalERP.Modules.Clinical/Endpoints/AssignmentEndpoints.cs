using DentalERP.Modules.Clinical.Features.Assignments.AssignDoctor;
using DentalERP.Modules.Clinical.Features.Assignments.TransferDoctor;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class AssignmentEndpoints
{
    public static IEndpointRouteBuilder MapAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients/{patientId}/doctors")
            .RequireAuthorization();

        group.MapPost("", async (Guid patientId, AssignDoctorRequest req, ISender sender,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new AssignDoctorCommand(
                patientId, req.DoctorId, req.IsPrimary, req.Notes, userId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/patients/{patientId}/doctors/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithName("AssignDoctor")
        .Produces(201).Produces(400).Produces(401).Produces(409);

        group.MapPatch("{assignmentId}/transfer",
            async (Guid patientId, Guid assignmentId, TransferDoctorRequest req, ISender sender,
                ClaimsPrincipal user, CancellationToken ct) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await sender.Send(new TransferDoctorCommand(
                    assignmentId, req.NewDoctorId, req.Reason, userId), ct);
                return result.IsSuccess
                    ? Results.Ok(new { newAssignmentId = result.Value })
                    : Results.BadRequest(result.Error);
            })
        .WithName("TransferDoctor")
        .Produces(200).Produces(400).Produces(401).Produces(404);

        return app;
    }
}

public sealed record AssignDoctorRequest(
    Guid DoctorId,
    bool IsPrimary = false,
    string? Notes = null);

public sealed record TransferDoctorRequest(
    Guid NewDoctorId,
    string? Reason = null);
