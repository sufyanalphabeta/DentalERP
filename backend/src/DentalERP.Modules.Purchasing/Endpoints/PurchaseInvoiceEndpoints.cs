using DentalERP.Modules.Purchasing.Features.CancelPurchaseInvoice;
using DentalERP.Modules.Purchasing.Features.CreatePurchaseInvoice;
using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoiceDetail;
using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoicePdf;
using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoices;
using DentalERP.Modules.Purchasing.Features.PostPurchaseInvoice;
using DentalERP.Modules.Purchasing.Features.UpdatePurchaseInvoice;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Endpoints;

public static class PurchaseInvoiceEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/purchasing/invoices");

        grp.MapGet("/", async (IMediator mediator,
            Guid? supplierId, string? status, string? search,
            DateOnly? from, DateOnly? to, int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetPurchaseInvoicesQuery(supplierId, status, search, from, to, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        }).RequirePermission("Purchasing.Invoices.View");

        grp.MapPost("/", async (IMediator mediator, CreatePurchaseInvoiceCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/invoices/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        }).RequirePermission("Purchasing.Invoices.Create");

        // Item search — must be before /{id:guid} to avoid route conflicts
        // Purchase invoices: items only (no medical services)
        grp.MapGet("/item-search", async (PurchasingDbContext db, CancellationToken ct, string? q, int limit = 20) =>
        {
            // When q is empty, return first 20 items (show on focus)
            var pattern = string.IsNullOrWhiteSpace(q) ? "%" : $"%{q.Trim()}%";
            var items = await db.Database.SqlQuery<ItemSearchRow>(
                $"""
                SELECT i.id AS "Id", COALESCE(i.item_code, '') AS "ItemCode", i.name AS "Name",
                       i.barcode AS "Barcode", i.unit_cost AS "UnitCost", i.sale_price AS "SalePrice",
                       'Item' AS "Kind",
                       COALESCE(SUM(CASE WHEN sb.is_depleted = false THEN sb.quantity ELSE 0 END), 0) AS "CurrentStock"
                FROM items i
                LEFT JOIN stock_batches sb ON sb.item_id = i.id
                WHERE i.deleted_at IS NULL AND i.is_active = true
                  AND (i.name ILIKE {pattern}
                       OR (i.barcode IS NOT NULL AND i.barcode ILIKE {pattern})
                       OR i.item_code ILIKE {pattern})
                GROUP BY i.id, i.item_code, i.name, i.barcode, i.unit_cost, i.sale_price
                ORDER BY i.name
                LIMIT {limit}
                """).ToListAsync(ct);

            return Results.Ok(items);
        });

        grp.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetPurchaseInvoiceDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        }).RequirePermission("Purchasing.Invoices.View");

        grp.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdatePurchaseInvoiceCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { InvoiceId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        }).RequirePermission("Purchasing.Invoices.Edit");

        grp.MapPost("/{id:guid}/post", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new PostPurchaseInvoiceCommand(id));
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        }).RequirePermission("Purchasing.Orders.Approve");

        grp.MapPost("/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelPurchaseInvoiceCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { InvoiceId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = r.Error.Message });
        }).RequirePermission("Purchasing.Invoices.Delete");

        grp.MapGet("/{id:guid}/pdf", async (IMediator mediator, Guid id, string? clinicName) =>
        {
            var r = await mediator.Send(new GetPurchaseInvoicePdfQuery(id, clinicName ?? "عيادة الأسنان"));
            return r.IsSuccess
                ? Results.File(r.Value, "application/pdf", $"purchase-invoice-{id}.pdf")
                : Results.NotFound(r.Error);
        }).RequirePermission("Purchasing.Invoices.ExportPdf");

        grp.MapDelete("/{id:guid}", async (PurchasingDbContext db, Guid id, CancellationToken ct) =>
        {
            var inv = await db.PurchaseInvoices.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (inv is null) return Results.NotFound();
            if (inv.Status == "Posted")
                return Results.BadRequest(new { error = "لا يمكن حذف الفاتورة المرحّلة. قم بإلغاء الترحيل أولاً." });
            inv.Delete();
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequirePermission("Purchasing.Invoices.Delete");

        return app;
    }
}

internal sealed record ItemSearchRow(Guid Id, string ItemCode, string Name, string? Barcode, decimal UnitCost, decimal SalePrice, string Kind, decimal CurrentStock);
