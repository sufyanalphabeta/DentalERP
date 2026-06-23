using DentalERP.Modules.Inventory.Features.AddItemBarcode;
using DentalERP.Modules.Inventory.Features.CreateItem;
using DentalERP.Modules.Inventory.Features.CreateItemCategory;
using DentalERP.Modules.Inventory.Features.CreateWarehouse;
using DentalERP.Modules.Inventory.Features.GetItemCategories;
using DentalERP.Modules.Inventory.Features.GetItemDetail;
using DentalERP.Modules.Inventory.Features.GetItems;
using DentalERP.Modules.Inventory.Features.GetMovements;
using DentalERP.Modules.Inventory.Features.GetStockAlerts;
using DentalERP.Modules.Inventory.Features.GetUnitsOfMeasure;
using DentalERP.Modules.Inventory.Features.GetWarehouses;
using DentalERP.Modules.Inventory.Features.LookupItemByBarcode;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var inv = app.MapGroup("/api/inventory");

        inv.MapGet("/items", async (IMediator mediator,
            string? search, Guid? categoryId, string? barcode,
            bool? lowStock, bool? activeOnly, int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetItemsQuery(search, categoryId, barcode, lowStock, activeOnly, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.View");

        inv.MapPost("/items", async (IMediator mediator, CreateItemCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/inventory/items/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.Create");

        inv.MapGet("/items/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetItemDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        }).RequirePermission("Inventory.Items.View");

        inv.MapGet("/items/by-barcode/{barcode}", async (IMediator mediator, string barcode) =>
        {
            var r = await mediator.Send(new LookupItemByBarcodeQuery(barcode));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        }).RequirePermission("Inventory.Items.View");

        inv.MapPut("/items/{id:guid}", async (InventoryDbContext db, Guid id, UpdateItemRequest req, CancellationToken ct) =>
        {
            var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (item is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "اسم الصنف مطلوب" });

            item.Update(req.Name, req.NameAr, req.CategoryId, req.UnitOfMeasureId,
                req.ReorderLevel, req.ReorderQuantity, req.IsExpiryTracked,
                req.AllowNegativeStock, req.StorageConditions, req.Notes);

            if (req.UnitCost.HasValue)   item.UpdateCost(req.UnitCost.Value);
            if (req.SalePrice.HasValue)  item.UpdateSalePrice(req.SalePrice.Value);

            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequirePermission("Inventory.Items.Edit");

        inv.MapPost("/items/{id:guid}/barcodes", async (IMediator mediator, Guid id, AddItemBarcodeCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ItemId = id });
            return r.IsSuccess
                ? Results.Created($"/api/inventory/items/{id}/barcodes/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.Edit");

        inv.MapGet("/stock/alerts", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetStockAlertsQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Alerts.View");

        inv.MapGet("/movements", async (IMediator mediator,
            Guid? itemId, string? movementType, string? destinationType,
            Guid? destinationId, DateTime? from, DateTime? to, int page = 1, int pageSize = 30) =>
        {
            var r = await mediator.Send(new GetMovementsQuery(itemId, movementType, destinationType, destinationId, from, to, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Movements.View");

        inv.MapGet("/warehouses", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetWarehousesQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.View");

        inv.MapPost("/warehouses", async (IMediator mediator, CreateWarehouseCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/inventory/warehouses/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.Create");

        inv.MapGet("/item-categories", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetItemCategoriesQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.View");

        inv.MapPost("/item-categories", async (IMediator mediator, CreateItemCategoryCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/inventory/item-categories/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.Create");

        inv.MapPut("/item-categories/{id:guid}", async (InventoryDbContext db, Guid id, UpdateCategoryRequest req, CancellationToken ct) =>
        {
            var cat = await db.ItemCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cat is null) return Results.NotFound();
            if (string.IsNullOrWhiteSpace(req.Name)) return Results.BadRequest(new { error = "اسم الفئة مطلوب" });
            cat.Update(req.Name, req.NameAr, cat.ParentId);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequirePermission("Inventory.Items.Edit");

        inv.MapDelete("/item-categories/{id:guid}", async (InventoryDbContext db, Guid id, CancellationToken ct) =>
        {
            var cat = await db.ItemCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (cat is null) return Results.NotFound();
            cat.Delete();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequirePermission("Inventory.Items.Delete");

        inv.MapGet("/units-of-measure", async (IMediator mediator) =>
        {
            var r = await mediator.Send(new GetUnitsOfMeasureQuery());
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Inventory.Items.View");

        return app;
    }
}

file sealed record UpdateItemRequest(
    string Name, string? NameAr,
    Guid? CategoryId, Guid? UnitOfMeasureId,
    decimal ReorderLevel, decimal ReorderQuantity,
    bool IsExpiryTracked, bool AllowNegativeStock,
    string? StorageConditions, string? Notes,
    decimal? UnitCost, decimal? SalePrice);

file sealed record UpdateCategoryRequest(string Name, string? NameAr);
