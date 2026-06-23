using DentalERP.Modules.Laboratory.Features.CancelLabOrder;
using DentalERP.Modules.Laboratory.Features.CompleteLabOrder;
using DentalERP.Modules.Laboratory.Features.CreateExternalLab;
using DentalERP.Modules.Laboratory.Features.CreateLabOrder;
using DentalERP.Modules.Laboratory.Features.GetExternalLabs;
using DentalERP.Modules.Laboratory.Features.GetLabOrderById;
using DentalERP.Modules.Laboratory.Features.GetLabOrders;
using DentalERP.Modules.Laboratory.Features.RecordLabResult;
using DentalERP.Modules.Laboratory.Features.SendLabOrder;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Laboratory.Endpoints;

public static class LabEndpoints
{
    public static IEndpointRouteBuilder MapLabEndpoints(this IEndpointRouteBuilder app)
    {
        var lab = app.MapGroup("/api/lab");

        lab.MapGet("/orders", async (IMediator mediator,
            Guid? patientId, Guid? doctorId, string? status,
            DateTime? from, DateTime? to, int page = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetLabOrdersQuery(patientId, doctorId, status, from, to, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Orders.View");

        lab.MapPost("/orders", async (IMediator mediator, CreateLabOrderCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/lab/orders/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Orders.Create");

        lab.MapGet("/orders/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetLabOrderByIdQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        }).RequirePermission("Lab.Orders.View");

        lab.MapPost("/orders/{id:guid}/send", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new SendLabOrderCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Orders.Edit");

        lab.MapPost("/orders/{id:guid}/result", async (IMediator mediator, Guid id, RecordLabResultRequest req) =>
        {
            var result = await mediator.Send(new RecordLabResultCommand(id, req.ResultNotes, req.StorageBucket, req.StorageKey, req.FileName, req.FileSize, req.ReceivedById));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Results.Edit");

        lab.MapPost("/orders/{id:guid}/complete", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new CompleteLabOrderCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Orders.Edit");

        lab.MapPost("/orders/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelRequest req) =>
        {
            var result = await mediator.Send(new CancelLabOrderCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.Orders.Delete");

        lab.MapGet("/external-labs", async (IMediator mediator, bool activeOnly = true) =>
        {
            var result = await mediator.Send(new GetExternalLabsQuery(activeOnly));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.ExternalLabs.View");

        lab.MapPost("/external-labs", async (IMediator mediator, CreateExternalLabCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/lab/external-labs/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Lab.ExternalLabs.Create");

        return app;
    }

    private sealed record RecordLabResultRequest(string? ResultNotes, string? StorageBucket, string? StorageKey, string? FileName, long? FileSize, Guid? ReceivedById);
    private sealed record CancelRequest(string Reason);
}
