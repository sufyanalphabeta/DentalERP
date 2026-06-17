using DentalERP.Modules.Patients.Features.Queue.CheckIn;
using DentalERP.Modules.Patients.Features.Queue.GetQueue;
using DentalERP.Modules.Patients.Features.Queue.UpdateQueueStatus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Patients.Endpoints;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/queue").RequireAuthorization();

        group.MapGet("/", async (
            string? date, Guid? doctorId,
            IMediator mediator, CancellationToken ct) =>
        {
            DateOnly? d = date != null ? DateOnly.Parse(date) : null;
            var result = await mediator.Send(new GetQueueQuery(d, doctorId), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/check-in", async (CheckInCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/queue/{result.Value!.QueueEntryId}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapPatch("/{id:guid}/status", async (
            Guid id, QueueStatusRequest request,
            IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new UpdateQueueStatusCommand(id, request.Status), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });
    }
}

public sealed record QueueStatusRequest(string Status);
