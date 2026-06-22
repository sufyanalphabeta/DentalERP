using DentalERP.Modules.Financial.Features.Services.CreateService;
using DentalERP.Modules.Financial.Features.Services.CreateServiceCategory;
using DentalERP.Modules.Financial.Features.Services.GetServiceCategories;
using DentalERP.Modules.Financial.Features.Services.GetServices;
using DentalERP.Modules.Financial.Features.Services.ToggleServiceCategory;
using DentalERP.Modules.Financial.Features.Services.UpdateService;
using DentalERP.Modules.Financial.Features.Services.UpdateServiceCategory;
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

        // Service categories
        group.MapGet("/categories", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceCategoriesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/categories", async (IMediator mediator, CreateServiceCategoryRequest req) =>
        {
            var result = await mediator.Send(new CreateServiceCategoryCommand(req.Name, req.SortOrder));
            return result.IsSuccess
                ? Results.Created($"/api/services/categories/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error.Message });
        });

        group.MapPut("/categories/{id:guid}", async (IMediator mediator, Guid id, CreateServiceCategoryRequest req) =>
        {
            var result = await mediator.Send(new UpdateServiceCategoryCommand(id, req.Name, req.SortOrder));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error.Message });
        });

        group.MapPost("/categories/{id:guid}/toggle", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new ToggleServiceCategoryCommand(id));
            return result.IsSuccess ? Results.Ok(new { isActive = result.Value }) : Results.BadRequest(new { error = result.Error.Message });
        });

        // Services
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

    private sealed record CreateServiceCategoryRequest(string Name, short SortOrder = 0);
    private sealed record UpdateServiceRequest(string Name, decimal Price, Guid? CategoryId, string? Code);
}
