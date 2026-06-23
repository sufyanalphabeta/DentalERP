using DentalERP.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DentalERP.SharedKernel.Extensions;

public static class EndpointPermissionExtensions
{
    /// <summary>
    /// Requires authentication AND a specific permission claim in the JWT.
    /// Returns 401 if not authenticated, 403 if authenticated but permission missing.
    /// </summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.RequireAuthorization();
        builder.AddEndpointFilter(async (ctx, next) =>
        {
            var currentUser = ctx.HttpContext.RequestServices.GetService(typeof(ICurrentUser)) as ICurrentUser;
            if (currentUser is null || !currentUser.IsAuthenticated)
                return Microsoft.AspNetCore.Http.Results.Unauthorized();
            if (!currentUser.HasPermission(permission))
                return Microsoft.AspNetCore.Http.Results.Forbid();
            return await next(ctx);
        });
        return builder;
    }

    /// <summary>
    /// Requires authentication AND any one of the given permissions.
    /// </summary>
    public static TBuilder RequireAnyPermission<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.RequireAuthorization();
        builder.AddEndpointFilter(async (ctx, next) =>
        {
            var currentUser = ctx.HttpContext.RequestServices.GetService(typeof(ICurrentUser)) as ICurrentUser;
            if (currentUser is null || !currentUser.IsAuthenticated)
                return Microsoft.AspNetCore.Http.Results.Unauthorized();
            if (!permissions.Any(p => currentUser.HasPermission(p)))
                return Microsoft.AspNetCore.Http.Results.Forbid();
            return await next(ctx);
        });
        return builder;
    }
}
