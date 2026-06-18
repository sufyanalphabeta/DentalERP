using DentalERP.Modules.Financial.Features.Invoices.CancelInvoice;
using DentalERP.Modules.Financial.Features.Invoices.CreateInvoice;
using DentalERP.Modules.Financial.Features.Invoices.GetInvoice;
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
            DateTime? from, DateTime? to, int page = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetInvoicesQuery(patientId, doctorId, status, from, to, page, pageSize));
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

        group.MapPost("/{id:guid}/cancel", async (IMediator mediator, Guid id, CancelRequest req) =>
        {
            var result = await mediator.Send(new CancelInvoiceCommand(id, req.Reason));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        return app;
    }

    private sealed record CancelRequest(string Reason);
}
