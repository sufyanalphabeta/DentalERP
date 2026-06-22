using DentalERP.Modules.Purchasing.Features.AddSupplierItemCode;
using DentalERP.Modules.Purchasing.Features.CreateSupplier;
using DentalERP.Modules.Purchasing.Features.GetSupplierBalance;
using DentalERP.Modules.Purchasing.Features.GetSupplierDetail;
using DentalERP.Modules.Purchasing.Features.GetSupplierItemCatalog;
using DentalERP.Modules.Purchasing.Features.GetSuppliers;
using DentalERP.Modules.Purchasing.Features.GetSupplierStatement;
using DentalERP.Modules.Purchasing.Features.GetSupplierStatementPdf;
using DentalERP.Modules.Purchasing.Features.LookupBySupplierCode;
using DentalERP.Modules.Purchasing.Features.UpdateSupplier;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Purchasing.Endpoints;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var sup = app.MapGroup("/api/suppliers").RequireAuthorization();

        sup.MapGet("/", async (IMediator mediator,
            string? search, bool? activeOnly, string? category, int page = 1, int pageSize = 20) =>
        {
            var r = await mediator.Send(new GetSuppliersQuery(search, activeOnly, category, page, pageSize));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        sup.MapPost("/", async (IMediator mediator, CreateSupplierCommand cmd) =>
        {
            var r = await mediator.Send(cmd);
            return r.IsSuccess
                ? Results.Created($"/api/suppliers/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        sup.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetSupplierDetailQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        sup.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateSupplierCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { SupplierId = id });
            return r.IsSuccess ? Results.NoContent() : Results.BadRequest(r.Error);
        });

        sup.MapGet("/{id:guid}/balance", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetSupplierBalanceQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        sup.MapGet("/{id:guid}/statement", async (IMediator mediator, Guid id,
            DateOnly? from, DateOnly? to) =>
        {
            var fromDt = from.HasValue
                ? new DateTime(from.Value.Year, from.Value.Month, from.Value.Day, 0, 0, 0, DateTimeKind.Utc)
                : (DateTime?)null;
            var toDt = to.HasValue
                ? new DateTime(to.Value.Year, to.Value.Month, to.Value.Day, 23, 59, 59, DateTimeKind.Utc)
                : (DateTime?)null;
            var r = await mediator.Send(new GetSupplierStatementQuery(id, fromDt, toDt));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        sup.MapGet("/{id:guid}/statement/pdf", async (IMediator mediator, Guid id, DateOnly? from, DateOnly? to, string? clinicName) =>
        {
            var fromDt = from.HasValue ? new DateTime(from.Value.Year, from.Value.Month, from.Value.Day, 0, 0, 0, DateTimeKind.Utc) : (DateTime?)null;
            var toDt = to.HasValue ? new DateTime(to.Value.Year, to.Value.Month, to.Value.Day, 23, 59, 59, DateTimeKind.Utc) : (DateTime?)null;
            var r = await mediator.Send(new GetSupplierStatementPdfQuery(id, fromDt, toDt, clinicName ?? "عيادة الأسنان"));
            return r.IsSuccess
                ? Results.File(r.Value, "application/pdf", $"supplier-statement-{id}.pdf")
                : Results.NotFound(r.Error);
        });

        sup.MapGet("/{id:guid}/catalog", async (IMediator mediator, Guid id) =>
        {
            var r = await mediator.Send(new GetSupplierItemCatalogQuery(id));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
        });

        sup.MapPost("/{id:guid}/catalog", async (IMediator mediator, Guid id, AddSupplierItemCodeCommand cmd) =>
        {
            var r = await mediator.Send(cmd with { SupplierId = id });
            return r.IsSuccess
                ? Results.Created($"/api/suppliers/{id}/catalog/{r.Value}", new { id = r.Value })
                : Results.BadRequest(r.Error);
        });

        sup.MapGet("/{id:guid}/catalog/lookup", async (IMediator mediator, Guid id, string code) =>
        {
            var r = await mediator.Send(new LookupBySupplierCodeQuery(id, code));
            return r.IsSuccess ? Results.Ok(r.Value) : Results.NotFound(r.Error);
        });

        return app;
    }
}
