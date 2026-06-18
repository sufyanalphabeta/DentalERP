using DentalERP.Modules.Inventory.Features.AddItemBarcode;
using DentalERP.Modules.Inventory.Features.CreateAdjustment;
using DentalERP.Modules.Inventory.Features.CreateItem;
using DentalERP.Modules.Inventory.Features.CreateItemCategory;
using DentalERP.Modules.Inventory.Features.CreateManualIssue;
using DentalERP.Modules.Inventory.Features.CreateWarehouse;
using DentalERP.Modules.Inventory.Features.GetItemCategories;
using DentalERP.Modules.Inventory.Features.GetItemDetail;
using DentalERP.Modules.Inventory.Features.GetItems;
using DentalERP.Modules.Inventory.Features.GetMovements;
using DentalERP.Modules.Inventory.Features.GetStockAlerts;
using DentalERP.Modules.Inventory.Features.GetUnitsOfMeasure;
using DentalERP.Modules.Inventory.Features.GetWarehouses;
using DentalERP.Modules.Inventory.Features.LookupItemByBarcode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Inventory.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var inv = app.MapGroup("/api/inventory").RequireAuthorization();

        // Items
        inv.MapGet("/items", async (IMediator mediator,
            string? search, Guid? categoryId, string? barcode,
            bool? lowStock, bool? activeOnly, int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetItemsQuery(search, categoryId, barcode, lowStock, activeOnly, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        inv.MapPost("/items", async (IMediator mediator, CreateItemCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Created($"/api/inventory/items/{r.Value}", new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        inv.MapGet("/items/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetItemDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        inv.MapGet("/items/by-barcode/{barcode}", async (IMediator mediator, string barcode) =>
        {
            var r = await mediator.Send(new LookupItemByBarcodeQuery(barcode));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        inv.MapPost("/items/{id:guid}/barcodes", async (IMediator mediator, Guid id, AddItemBarcodeCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ItemId = id });
            return r.IsSuccess ? Results.Created($"/api/inventory/items/{id}/barcodes/{r.Value}", new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        inv.MapPost("/items/{id:guid}/adjust", async (IMediator mediator, Guid id, CreateAdjustmentCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ItemId = id });
            return r.IsSuccess ? Results.Ok(new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        // Stock
        inv.MapGet("/stock/alerts", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetStockAlertsQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        inv.MapPost("/stock/issue", async (IMediator mediator, CreateManualIssueCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Ok(new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        // Movements
        inv.MapGet("/movements", async (IMediator mediator,
            Guid? itemId, string? movementType, string? destinationType,
            Guid? destinationId, DateTime? from, DateTime? to, int page = 1, int pageSize = 30) =>
        {
            var r = await mediator.Send(new GetMovementsQuery(itemId, movementType, destinationType, destinationId, from, to, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        // Warehouses
        inv.MapGet("/warehouses", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetWarehousesQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        inv.MapPost("/warehouses", async (IMediator mediator, CreateWarehouseCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Created($"/api/inventory/warehouses/{r.Value}", new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        // Categories
        inv.MapGet("/item-categories", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetItemCategoriesQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        inv.MapPost("/item-categories", async (IMediator mediator, CreateItemCategoryCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess ? Results.Created($"/api/inventory/item-categories/{r.Value}", new { id = r.Value }) : Results.BadRequest(r.Error);
        });

        // Units of Measure
        inv.MapGet("/units-of-measure", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetUnitsOfMeasureQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        return app;
    }
}
