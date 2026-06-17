using DentalERP.Modules.IAM.Features.Users.CreateUser;
using DentalERP.Modules.IAM.Features.Users.DeleteUser;
using DentalERP.Modules.IAM.Features.Users.GetUser;
using DentalERP.Modules.IAM.Features.Users.GetUsers;
using DentalERP.Modules.IAM.Features.Users.ToggleUser;
using DentalERP.Modules.IAM.Features.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.IAM.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] GetUsersQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        .WithSummary("Get all users (paginated)");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetUserQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .WithSummary("Get user by ID");

        group.MapPost("/", async (CreateUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .WithSummary("Create a new user");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { UserId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .WithSummary("Update user");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUserCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .WithSummary("Soft-delete user");

        group.MapPatch("/{id:guid}/toggle", async (Guid id, ToggleUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { UserId = id });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .WithSummary("Enable or disable user");

        return app;
    }
}
