using DentalERP.Modules.IAM.Features.Roles.CreateRole;
using DentalERP.Modules.IAM.Features.Roles.DeleteRole;
using DentalERP.Modules.IAM.Features.Roles.GetRole;
using DentalERP.Modules.IAM.Features.Roles.GetRoles;
using DentalERP.Modules.IAM.Features.Roles.UpdateRole;
using DentalERP.Modules.IAM.Features.Permissions;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.IAM.Endpoints;

public static class RolesEndpoints
{
    public static IEndpointRouteBuilder MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").WithTags("Roles");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRolesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Roles.View").WithSummary("Get all roles");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetRoleQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        }).RequirePermission("IAM.Roles.View").WithSummary("Get role by ID");

        group.MapPost("/", async (CreateRoleCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/roles/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Roles.Create").WithSummary("Create a new role");

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoleCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { RoleId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Roles.Edit").WithSummary("Update role and its permissions");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRoleCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Roles.Delete").WithSummary("Delete role");

        app.MapGet("/api/permissions", async (ISender sender) =>
        {
            var result = await sender.Send(new GetPermissionsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .WithTags("Roles")
        .RequirePermission("IAM.Roles.View")
        .WithSummary("Get all permissions grouped by module");

        return app;
    }
}
