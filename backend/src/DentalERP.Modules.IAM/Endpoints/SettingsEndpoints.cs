using DentalERP.Modules.IAM.Features.Settings;
using DentalERP.SharedKernel.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.IAM.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGet("/", async (string? group_, ISender sender) =>
        {
            var result = await sender.Send(new GetSettingsQuery(group_));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Settings.View").WithSummary("Get system settings");

        group.MapPut("/{key}", async (string key, UpdateSettingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command with { Key = key });
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequirePermission("IAM.Settings.Edit").WithSummary("Update a system setting");

        return app;
    }
}
