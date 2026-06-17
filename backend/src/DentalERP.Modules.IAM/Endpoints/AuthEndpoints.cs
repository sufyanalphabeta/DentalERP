using DentalERP.Modules.IAM.Features.Auth.ChangePassword;
using DentalERP.Modules.IAM.Features.Auth.Login;
using DentalERP.Modules.IAM.Features.Auth.Logout;
using DentalERP.Modules.IAM.Features.Auth.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.IAM.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithSummary("Login with username and password");

        group.MapPost("/refresh-token", async (RefreshTokenCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithSummary("Refresh access token");

        group.MapPost("/logout", async (LogoutCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithSummary("Logout and revoke refresh token");

        group.MapPost("/change-password", async (ChangePasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization()
        .WithSummary("Change current user password");

        return app;
    }
}
