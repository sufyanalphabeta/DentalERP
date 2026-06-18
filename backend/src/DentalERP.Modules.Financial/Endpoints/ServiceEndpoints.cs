using DentalERP.Modules.Financial.Features.Services.CreateService;
using DentalERP.Modules.Financial.Features.Services.GetServices;
using DentalERP.Modules.Financial.Features.Services.UpdateService;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Financial.Endpoints;

public static class ServiceEndpoints
{
    public static IEndpointRouteBuilder MapServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/services").RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, string? search, Guid? categoryId, bool activeOnly = true) =>
        {
            var result = await mediator.Send(new GetServicesQuery(search, categoryId, activeOnly));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/", async (IMediator mediator, CreateServiceCommand cmd) =>
        {
            var result = await mediator.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/services/{result.Value}", new { id = result.Value }) : Results.BadRequest(result.Error);
        });

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateServiceRequest req) =>
        {
            var result = await mediator.Send(new UpdateServiceCommand(id, req.Name, req.Price, req.CategoryId, req.Code));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        return app;
    }

    private sealed record UpdateServiceRequest(string Name, decimal Price, Guid? CategoryId, string? Code);
}
