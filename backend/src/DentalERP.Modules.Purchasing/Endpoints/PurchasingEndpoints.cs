using DentalERP.Modules.Purchasing.Features.ApprovePurchaseOrder;
using DentalERP.Modules.Purchasing.Features.CancelPurchaseOrder;
using DentalERP.Modules.Purchasing.Features.CompletePurchaseReturn;
using DentalERP.Modules.Purchasing.Features.ConfirmPurchaseReturn;
using DentalERP.Modules.Purchasing.Features.CreateGoodsReceipt;
using DentalERP.Modules.Purchasing.Features.CreatePurchaseOrder;
using DentalERP.Modules.Purchasing.Features.CreatePurchaseReturn;
using DentalERP.Modules.Purchasing.Features.GeneratePurchaseReturnVoucher;
using DentalERP.Modules.Purchasing.Features.GetGoodsReceiptDetail;
using DentalERP.Modules.Purchasing.Features.GetPODetail;
using DentalERP.Modules.Purchasing.Features.GetPurchaseOrders;
using DentalERP.Modules.Purchasing.Features.GetPurchaseReturnDetail;
using DentalERP.Modules.Purchasing.Features.GetPurchaseReturns;
using DentalERP.Modules.Purchasing.Features.MarkPOSent;
using DentalERP.Modules.Purchasing.Features.RecordSupplierPayment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Purchasing.Endpoints;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder app)
    {
        var grp = app.MapGroup("/api/purchasing").RequireAuthorization();

        // Purchase Orders
        grp.MapGet("/purchase-orders", async (IMediator mediator,
            Guid? supplierId, string? status, DateTime? from, DateTime? to,
            int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetPurchaseOrdersQuery(supplierId, status, from, to, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-orders", async (IMediator mediator, CreatePurchaseOrderCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/purchase-orders/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        grp.MapGet("/purchase-orders/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetPODetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        grp.MapPost("/purchase-orders/{id:guid}/approve", async (IMediator mediator, Guid id, ApprovePurchaseOrderCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { PoId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-orders/{id:guid}/send", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new MarkPOSentCommand(id));
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        grp.MapPost("/purchase-orders/{id:guid}/cancel", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new CancelPurchaseOrderCommand(id));
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        // Goods Receipts
        grp.MapPost("/goods-receipts", async (IMediator mediator, CreateGoodsReceiptCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/goods-receipts/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        grp.MapGet("/goods-receipts/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetGoodsReceiptDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        // Supplier Payments
        grp.MapPost("/supplier-payments", async (IMediator mediator, RecordSupplierPaymentCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/purchasing/supplier-payments/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        // Purchase Returns
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

        grp.MapGet("/purchase-returns/{id:guid}/voucher", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GeneratePurchaseReturnVoucherQuery(id));
            if (r.IsFailure) return Results.NotFound(r.Error);
            return Results.File(r.Value, "application/pdf", $"return-voucher-{id}.pdf");
        });

        return app;
    }
}
