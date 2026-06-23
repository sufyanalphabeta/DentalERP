using DentalERP.Modules.Assets.Features.AddAssetDocument;
using DentalERP.Modules.Assets.Features.CreateAsset;
using DentalERP.Modules.Assets.Features.CreateAssetCategory;
using DentalERP.Modules.Assets.Features.CreateAssetMaintenance;
using DentalERP.Modules.Assets.Features.DisposeAsset;
using DentalERP.Modules.Assets.Features.GenerateAssetRegister;
using DentalERP.Modules.Assets.Features.GetAssetByTag;
using DentalERP.Modules.Assets.Features.GetAssetCategories;
using DentalERP.Modules.Assets.Features.GetAssetDetail;
using DentalERP.Modules.Assets.Features.GetAssetDocuments;
using DentalERP.Modules.Assets.Features.GetAssetMaintenances;
using DentalERP.Modules.Assets.Features.GetAssets;
using DentalERP.Modules.Assets.Features.UpdateAsset;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Assets.Endpoints;

internal static class AssetsEndpoints
{
    internal static void MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/assets");

        grp.MapGet("/categories", async (bool? activeOnly, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetCategoriesQuery(activeOnly ?? false));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Categories.View").WithName("GetAssetCategories");

        grp.MapPost("/categories", async (CreateAssetCategoryCommand cmd, IMediator mediator) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Created($"/api/assets/categories/{r.Value}", r.Value) : Results.Conflict(r.Error);
        }).RequirePermission("Assets.Categories.Create").WithName("CreateAssetCategory");

        grp.MapGet("/", async (Guid? categoryId, string? status, string? search, int page, int pageSize, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetsQuery(categoryId, status, search,
                page < 1 ? 1 : page, pageSize < 1 ? 50 : pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.View").WithName("GetAssets");

        grp.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        }).RequirePermission("Assets.Assets.View").WithName("GetAssetDetail");

        grp.MapGet("/by-tag/{tag}", async (string tag, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetByTagQuery(tag));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        }).RequirePermission("Assets.Assets.View").WithName("GetAssetByTag");

        grp.MapPost("/", async (CreateAssetCommand cmd, IMediator mediator) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Created($"/api/assets/{r.Value.Id}", r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.Create").WithName("CreateAsset");

        grp.MapPut("/{id:guid}", async (Guid id, UpdateAssetRequest req, IMediator mediator) =>
        {
            var r = await mediator.Send(new UpdateAssetCommand(id, req.Name, req.CategoryId,
                req.PurchaseDate, req.PurchaseCost, req.Location, req.Notes));
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.Edit").WithName("UpdateAsset");

        grp.MapPost("/{id:guid}/dispose", async (Guid id, DisposeAssetRequest req, IMediator mediator) =>
        {
            var r = await mediator.Send(new DisposeAssetCommand(id, req.DisposedById));
            return r.IsSuccess ? Results.Ok() : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.Delete").WithName("DisposeAsset");

        grp.MapGet("/{id:guid}/documents", async (Guid id, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetDocumentsQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.View").WithName("GetAssetDocuments");

        grp.MapPost("/{id:guid}/documents", async (Guid id, IFormFile file,
            string? notes, Guid? uploadedById, IMediator mediator) =>
        {
            using var stream = file.OpenReadStream();
            var r = await mediator.Send(new AddAssetDocumentCommand(
                id, file.FileName, stream, file.ContentType, notes, uploadedById));
            return r.IsSuccess ? Results.Created($"/api/assets/{id}/documents/{r.Value}", r.Value)
                : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.Edit").WithName("AddAssetDocument").DisableAntiforgery();

        grp.MapGet("/{id:guid}/maintenances", async (Guid id, IMediator mediator) =>
        {
            var r = await mediator.Send(new GetAssetMaintenancesQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Maintenance.View").WithName("GetAssetMaintenances");

        grp.MapPost("/{id:guid}/maintenances", async (Guid id, CreateMaintenanceRequest req, IMediator mediator) =>
        {
            var r = await mediator.Send(new CreateAssetMaintenanceCommand(
                id, req.MaintenanceDate, req.Cost, req.Description,
                req.Vendor, req.VaultId, req.CostCategoryId, req.CreatedById));
            return r.IsSuccess ? Results.Created($"/api/assets/{id}/maintenances/{r.Value}", r.Value)
                : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Maintenance.Create").WithName("CreateAssetMaintenance");

        grp.MapGet("/register/pdf", async (Guid? categoryId, string? status, IMediator mediator) =>
        {
            var r = await mediator.Send(new GenerateAssetRegisterQuery(categoryId, status));
            return r.IsSuccess
                ? Results.File(r.Value, "application/pdf", $"asset-register-{DateTime.UtcNow:yyyyMMdd}.pdf")
                : Results.BadRequest(r.Error);
        }).RequirePermission("Assets.Assets.Print").WithName("GetAssetRegisterPdf");
    }
}

internal sealed record UpdateAssetRequest(
    string Name, Guid? CategoryId, DateOnly? PurchaseDate,
    decimal? PurchaseCost, string? Location, string? Notes
);
internal sealed record DisposeAssetRequest(Guid? DisposedById);
internal sealed record CreateMaintenanceRequest(
    DateOnly MaintenanceDate, decimal Cost, string Description,
    string? Vendor, Guid? VaultId, Guid? CostCategoryId, Guid? CreatedById
);

