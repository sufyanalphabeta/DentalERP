using DentalERP.Modules.Patients.Features.Patients.CreatePatient;
using DentalERP.Modules.Patients.Features.Patients.DeletePatient;
using DentalERP.Modules.Patients.Features.Patients.GetPatient;
using DentalERP.Modules.Patients.Features.Patients.GetPatients;
using DentalERP.Modules.Patients.Features.Patients.UpdatePatient;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Patients.Endpoints;

public static class PatientsEndpoints
{
    public static void MapPatientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patients").RequireAuthorization();

        group.MapGet("/", async (
            string? search, int? page, int? pageSize,
            IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new GetPatientsQuery(search, page ?? 1, pageSize ?? 20), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPatientQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapPost("/", async (CreatePatientCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/patients/{result.Value!.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:guid}", async (
            Guid id, UpdatePatientRequest request,
            IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdatePatientCommand(
                id, request.FullName, request.Phone, request.DateOfBirth,
                request.Gender, request.Phone2, request.Email, request.Address,
                request.NationalId, request.BloodType, request.Allergies,
                request.ChronicDiseases, request.Notes);
            var result = await mediator.Send(command, ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeletePatientCommand(id), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });
    }
}

public sealed record UpdatePatientRequest(
    string FullName,
    string Phone,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? Phone2 = null,
    string? Email = null,
    string? Address = null,
    string? NationalId = null,
    string? BloodType = null,
    string? Allergies = null,
    string? ChronicDiseases = null,
    string? Notes = null
);
