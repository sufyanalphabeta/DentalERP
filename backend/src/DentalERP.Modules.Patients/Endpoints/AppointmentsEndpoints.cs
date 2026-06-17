using DentalERP.Modules.Patients.Features.Appointments.CreateAppointment;
using DentalERP.Modules.Patients.Features.Appointments.GetAppointments;
using DentalERP.Modules.Patients.Features.Appointments.UpdateAppointmentStatus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Patients.Endpoints;

public static class AppointmentsEndpoints
{
    public static void MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").RequireAuthorization();

        group.MapGet("/", async (
            string? fromDate, string? toDate, Guid? doctorId, Guid? patientId,
            string? status, int? page, int? pageSize,
            IMediator mediator, CancellationToken ct) =>
        {
            DateOnly? from = fromDate != null ? DateOnly.Parse(fromDate) : null;
            DateOnly? to = toDate != null ? DateOnly.Parse(toDate) : null;
            var result = await mediator.Send(
                new GetAppointmentsQuery(from, to, doctorId, patientId, status, page ?? 1, pageSize ?? 50), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/", async (CreateAppointmentCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/appointments/{result.Value}", new { Id = result.Value })
                : Results.BadRequest(result.Error);
        });

        group.MapPatch("/{id:guid}/status", async (
            Guid id, UpdateStatusRequest request,
            IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new UpdateAppointmentStatusCommand(id, request.Status, request.CancellationReason), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });
    }
}

public sealed record UpdateStatusRequest(string Status, string? CancellationReason = null);
