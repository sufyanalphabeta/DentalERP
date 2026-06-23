using DentalERP.Modules.Financial.Features.Payments.AddPayment;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices");

        group.MapPost("/{id:guid}/payments", async (IMediator mediator, Guid id, AddPaymentRequest req) =>
        {
            var result = await mediator.Send(new AddPaymentCommand(
                id, req.VaultId, req.Amount, req.PaymentMethod,
                req.ReferenceNumber, req.Notes, req.CreatedByUserId));
            return result.IsSuccess ? Results.Created($"/api/payments/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        }).RequirePermission("Financial.Payments.Create");

        return app;
    }

    private sealed record AddPaymentRequest(
        Guid VaultId,
        decimal Amount,
        string PaymentMethod,
        string? ReferenceNumber,
        string? Notes,
        Guid? CreatedByUserId);
}
