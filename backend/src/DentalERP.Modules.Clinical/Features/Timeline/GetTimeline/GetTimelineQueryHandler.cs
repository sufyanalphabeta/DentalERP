using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Timeline.GetTimeline;

public sealed class GetTimelineQueryHandler(ClinicalDbContext db)
    : IRequestHandler<GetTimelineQuery, Result<GetTimelineResponse>>
{
    public async Task<Result<GetTimelineResponse>> Handle(GetTimelineQuery request, CancellationToken ct)
    {
        var query = db.PatientTimeline
            .Where(e => e.PatientId == request.PatientId);

        if (request.EventCategory is not null)
            query = query.Where(e => e.EventCategory == request.EventCategory);

        if (request.EventType is not null)
            query = query.Where(e => e.EventType == request.EventType);

        if (request.From.HasValue)
            query = query.Where(e => e.EventAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.EventAt <= request.To.Value);

        var total = await query.CountAsync(ct);

        var events = await query
            .OrderByDescending(e => e.EventAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new TimelineEventDto(
                e.Id,
                e.EventType,
                e.EventCategory,
                e.Title,
                e.Description,
                e.ActorName,
                e.ActorId,
                e.LinkedEntityType,
                e.LinkedEntityId,
                e.Metadata,
                e.EventAt,
                e.IsVisibleToDoctor,
                e.IsVisibleToPatient))
            .ToListAsync(ct);

        return Result.Success(new GetTimelineResponse(request.PatientId, total, events));
    }
}
