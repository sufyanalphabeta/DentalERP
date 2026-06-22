using DentalERP.Modules.Financial.Features.Invoices.CancelInvoice;
using DentalERP.Modules.Financial.Features.Invoices.ConfirmInvoice;
using DentalERP.Modules.Financial.Features.Invoices.CreateInvoice;
using DentalERP.Modules.Financial.Features.Invoices.GetInvoice;
using DentalERP.Modules.Financial.Features.Invoices.GetInvoicePdf;
using DentalERP.Modules.Financial.Features.Invoices.GetInvoices;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").RequireAuthorization();

        group.MapGet("/", async (IMediator mediator,
            Guid? patientId, Guid? doctorId, string? status,
            DateTime? from, DateTime? to, int page = 1, int pageSize = 20, string? search = null) =>
        {
            var result = await mediator.Send(new GetInvoicesQuery(patientId, doctorId, status, from, to, page, pageSize, search));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetInvoiceQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapPost("/", async (IMediator mediator, CreateInvoiceCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/invoices/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        group.MapPost("/{id:guid}/confirm", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new ConfirmInvoiceCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapPost("/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelRequest req) =>
        {
            var result = await mediator.Send(new CancelInvoiceCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}/pdf", async (IMediator mediator, Guid id, string? clinicName) =>
        {
            var result = await mediator.Send(new GetInvoicePdfQuery(id, clinicName ?? "عيادة الأسنان"));
            return result.IsSuccess
                ? Results.File(result.Value, "application/pdf", $"invoice-{id}.pdf")
                : Results.NotFound(result.Error);
        });

        return app;
    }

    private sealed record CancelRequest(string Reason);
}
