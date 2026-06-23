using DentalERP.Modules.IAM.Features.Users.CreateUser;
using DentalERP.Modules.IAM.Features.Users.DeleteUser;
using DentalERP.Modules.IAM.Features.Users.GetUser;
using DentalERP.Modules.IAM.Features.Users.GetUsers;
using DentalERP.Modules.IAM.Features.Users.ManageUserPermissions;
using DentalERP.Modules.IAM.Features.Users.ResetPassword;
using DentalERP.Modules.IAM.Features.Users.ToggleUser;
using DentalERP.Modules.IAM.Features.Users.UpdateUser;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.IAM.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async ([AsParameters] GetUsersQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.View")
        .WithSummary("Get all users (paginated)");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetUserQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .RequirePermission("IAM.Users.View")
        .WithSummary("Get user by ID");

        group.MapPost("/", async (CreateUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Create")
        .WithSummary("Create a new user");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { UserId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Edit")
        .WithSummary("Update user");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUserCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Delete")
        .WithSummary("Soft-delete user");

        group.MapPatch("/{id:guid}/toggle", async (Guid id, ToggleUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { UserId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Edit")
        .WithSummary("Enable or disable user");

        group.MapPost("/{id:guid}/reset-password", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ResetPasswordCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Edit")
        .WithSummary("Reset user password to 123456 and force change on next login");

        group.MapGet("/{id:guid}/effective-permissions", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetUserEffectivePermissionsQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .RequirePermission("IAM.Users.View")
        .WithSummary("Get effective permissions for a user (role + additional - denied)");

        group.MapPut("/{id:guid}/permissions", async (Guid id, SetUserPermissionsCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd with { UserId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequirePermission("IAM.Users.Edit")
        .WithSummary("Set per-user permission overrides (additional grants and explicit denies)");

        return app;
    }
}
