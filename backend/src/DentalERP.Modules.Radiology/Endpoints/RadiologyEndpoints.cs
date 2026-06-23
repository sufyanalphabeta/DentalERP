using DentalERP.Modules.Radiology.Features.CancelRadiologyOrder;
using DentalERP.Modules.Radiology.Features.CompleteRadiologyOrder;
using DentalERP.Modules.Radiology.Features.CreateRadiologyOrder;
using DentalERP.Modules.Radiology.Features.GetRadiologyOrderById;
using DentalERP.Modules.Radiology.Features.GetRadiologyOrders;
using DentalERP.Modules.Radiology.Features.GetRadiologyTypes;
using DentalERP.Modules.Radiology.Features.MarkRadiologyImaged;
using DentalERP.Modules.Radiology.Features.SaveRadiologyReport;
using DentalERP.Modules.Radiology.Features.UploadRadiologyImage;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Radiology.Endpoints;

public static class RadiologyEndpoints
{
    public static IEndpointRouteBuilder MapRadiologyEndpoints(this IEndpointRouteBuilder app)
    {
        var rad = app.MapGroup("/api/radiology");

        rad.MapGet("/types", async (IMediator mediator, bool activeOnly = true) =>
        {
            var result = await mediator.Send(new GetRadiologyTypesQuery(activeOnly));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.View");

        rad.MapGet("/orders", async (IMediator mediator,
            Guid? patientId, Guid? doctorId, string? status,
            DateTime? from, DateTime? to, int page = 1, int pageSize = 20, Guid? typeId = null) =>
        {
            var result = await mediator.Send(new GetRadiologyOrdersQuery(patientId, doctorId, status, from, to, page, pageSize, typeId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.View");

        rad.MapPost("/orders", async (IMediator mediator, CreateRadiologyOrderCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/radiology/orders/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.Create");

        rad.MapGet("/orders/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetRadiologyOrderByIdQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        }).RequirePermission("Radiology.Orders.View");

        rad.MapPost("/orders/{id:guid}/imaged", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new MarkRadiologyImagedCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Images.Upload");

        rad.MapPost("/orders/{id:guid}/images", async (IMediator mediator, Guid id, UploadImageRequest req) =>
        {
            var result = await mediator.Send(new UploadRadiologyImageCommand(id, req.StorageBucket, req.StorageKey, req.FileName, req.FileSize, req.ContentType, req.UploadedById));
            return result.IsSuccess ? Results.Created($"/api/radiology/orders/{id}/images/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Images.Upload");

        rad.MapPost("/orders/{id:guid}/report", async (IMediator mediator, Guid id, SaveReportRequest req) =>
        {
            var result = await mediator.Send(new SaveRadiologyReportCommand(id, req.ReportText, req.ReportedById));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.Edit");

        rad.MapPost("/orders/{id:guid}/complete", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new CompleteRadiologyOrderCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.Edit");

        rad.MapPost("/orders/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelRequest req) =>
        {
            var result = await mediator.Send(new CancelRadiologyOrderCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("Radiology.Orders.Delete");

        return app;
    }

    private sealed record UploadImageRequest(string StorageBucket, string StorageKey, string FileName, long FileSize, string? ContentType, Guid UploadedById);
    private sealed record SaveReportRequest(string ReportText, Guid ReportedById);
    private sealed record CancelRequest(string Reason);
}
