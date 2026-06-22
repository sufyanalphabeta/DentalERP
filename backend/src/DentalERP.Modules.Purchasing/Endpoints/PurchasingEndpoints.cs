using DentalERP.Modules.Purchasing.Features.CancelPurchaseReturn;
using DentalERP.Modules.Purchasing.Features.GetPurchasingReportPdf;
using DentalERP.Modules.Purchasing.Features.CompletePurchaseReturn;
using DentalERP.Modules.Purchasing.Features.ConfirmPurchaseReturn;
using DentalERP.Modules.Purchasing.Features.CreatePurchaseReturn;
using DentalERP.Modules.Purchasing.Features.DeleteSupplier;
using DentalERP.Modules.Purchasing.Features.GeneratePurchaseReturnVoucher;
using DentalERP.Modules.Purchasing.Features.GetPurchaseReturnDetail;
using DentalERP.Modules.Purchasing.Features.GetPurchaseReturns;
using DentalERP.Modules.Purchasing.Features.RecordSupplierPayment;
using DentalERP.Modules.Purchasing.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Endpoints;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/purchasing").RequireAuthorization();

        // ── Supplier Payments ────────────────────────────────────────────────
        grp.MapPost("/supplier-payments", async (IMediator mediator, RecordSupplierPaymentCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/supplier-payments/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        grp.MapGet("/supplier-payments", async (PurchasingDbContext db, CancellationToken ct,
            Guid? supplierId, int page = 1, int pageSize = 30) =>
        {
            var q = db.SupplierPayments.AsQueryable();
            if (supplierId.HasValue) q = q.Where(p => p.SupplierId == supplierId.Value);
            var total = await q.CountAsync(ct);
            var items = await q
                .OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new {
                    p.Id, p.PaymentNumber, p.SupplierId, p.VaultId,
                    p.Amount, p.PaymentDate, p.ReferenceNumber, p.Notes, p.CreatedAt
                })
                .ToListAsync(ct);
            return Results.Ok(new { items, total, page, pageSize });
        });

        // ── Purchase Returns ─────────────────────────────────────────────────
        grp.MapGet("/purchase-returns", async (IMediator mediator,
            Guid? supplierId, string? status, int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetPurchaseReturnsQuery(supplierId, status, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-returns", async (IMediator mediator, CreatePurchaseReturnCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/purchase-returns/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        grp.MapGet("/purchase-returns/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetPurchaseReturnDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        grp.MapPost("/purchase-returns/{id:guid}/confirm", async (IMediator mediator, Guid id, ConfirmPurchaseReturnCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ReturnId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-returns/{id:guid}/complete", async (IMediator mediator, Guid id, CompletePurchaseReturnCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ReturnId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-returns/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelPurchaseReturnCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { ReturnId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = r.Error.Message });
        });

        grp.MapDelete("/purchase-returns/{id:guid}", async (PurchasingDbContext db, Guid id, CancellationToken ct) =>
        {
            var ret = await db.PurchaseReturns.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (ret is null) return Results.NotFound();
            if (ret.Status != "Draft") return Results.BadRequest(new { error = "يمكن حذف المردودات في حالة المسودة فقط" });
            db.PurchaseReturns.Remove(ret);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        grp.MapGet("/purchase-returns/{id:guid}/voucher", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GeneratePurchaseReturnVoucherQuery(id));
            if (r.IsFailure) return Results.NotFound(r.Error);
            return Results.File(r.Value, "application/pdf", $"return-voucher-{id}.pdf");
        });

        // ── Purchasing Report PDF ────────────────────────────────────────────
        grp.MapGet("/invoices/report/pdf", async (IMediator mediator,
            Guid? supplierId, string? status,
            DateOnly? from, DateOnly? to, string? clinicName) =>
        {
            var r = await mediator.Send(new GetPurchasingReportPdfQuery(supplierId, status, from, to, clinicName ?? "عيادة الأسنان"));
            return r.IsSuccess
                ? Results.File(r.Value, "application/pdf", $"purchasing-report.pdf")
                : Results.BadRequest(r.Error);
        });

        // ── Supplier Deactivation / Deletion ─────────────────────────────────
        grp.MapDelete("/suppliers/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new DeleteSupplierCommand(id));
            return r.IsSuccess ? Results.Ok(new { result = r.Value }) : Results.BadRequest(new { error = r.Error.Message });
        });

        return app;
    }
}
