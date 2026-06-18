using DentalERP.Modules.Clinical.Features.Timeline.GetTimeline;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalERP.Modules.Clinical.Endpoints;

public static class TimelineEndpoints
{
    public static IEndpointRouteBuilder MapTimelineEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/patients/{patientId}/timeline",
            async (Guid patientId, ISender sender, CancellationToken ct,
                string? category = null,
                string? eventType = null,
                DateTime? from = null,
                DateTime? to = null,
                int page = 1,
                int pageSize = 50) =>
            {
                var result = await sender.Send(new GetTimelineQuery(
                    patientId, category, eventType, from, to, page, pageSize), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
            })
            .RequireAuthorization()
            .WithName("GetPatientTimeline")
            .Produces<GetTimelineResponse>()
            .Produces(401).Produces(404);

        return app;
    }
}
