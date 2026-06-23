using DentalERP.Modules.Patients.Features.Appointments.CreateAppointment;
using DentalERP.Modules.Patients.Features.Appointments.GetAppointments;
using DentalERP.Modules.Patients.Features.Appointments.UpdateAppointmentStatus;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Endpoints;

public static class AppointmentsEndpoints
{
    public static void MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/appointment-types", async (PatientsDbContext db, CancellationToken ct) =>
        {
            var types = await db.AppointmentTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .Select(t => new { t.Id, t.Name, t.NameAr, t.DefaultDurationMinutes, t.Color })
                .ToListAsync(ct);
            return Results.Ok(types);
        }).RequirePermission("Appointments.Appointments.View");

        var group = app.MapGroup("/api/appointments");

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
        }).RequirePermission("Appointments.Appointments.View");

        group.MapPost("/", async (CreateAppointmentCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/appointments/{result.Value}", new { Id = result.Value })
                : Results.BadRequest(result.Error);
        }).RequirePermission("Appointments.Appointments.Create");

        group.MapPatch("/{id:guid}/status", async (
            Guid id, UpdateStatusRequest request,
            IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new UpdateAppointmentStatusCommand(id, request.Status, request.CancellationReason), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAnyPermission("Appointments.Appointments.Edit", "Appointments.Appointments.Cancel");
    }
}

public sealed record UpdateStatusRequest(string Status, string? CancellationReason = null);
