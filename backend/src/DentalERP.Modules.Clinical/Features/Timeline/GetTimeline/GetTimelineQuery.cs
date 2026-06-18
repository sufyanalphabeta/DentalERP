using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Timeline.GetTimeline;

public sealed record GetTimelineQuery(
    Guid PatientId,
    string? EventCategory = null,
    string? EventType = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<GetTimelineResponse>>;

public sealed record GetTimelineResponse(
    Guid PatientId,
    int TotalCount,
    IReadOnlyList<TimelineEventDto> Events);

public sealed record TimelineEventDto(
    Guid Id,
    string EventType,
    string EventCategory,
    string Title,
    string? Description,
    string? ActorName,
    Guid? ActorId,
    string? LinkedEntityType,
    Guid? LinkedEntityId,
    string? Metadata,
    DateTime EventAt,
    bool IsVisibleToDoctor,
    bool IsVisibleToPatient);
