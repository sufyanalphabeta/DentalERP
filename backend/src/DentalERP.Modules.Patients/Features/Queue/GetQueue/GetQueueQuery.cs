using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Queue.GetQueue;

public sealed record GetQueueQuery(
    DateOnly? Date = null,
    Guid? DoctorId = null
) : IRequest<Result<GetQueueResponse>>;

public sealed record GetQueueResponse(
    DateOnly Date,
    IReadOnlyList<QueueEntryItem> Entries
);

public sealed record QueueEntryItem(
    Guid Id,
    int TokenNumber,
    Guid PatientId,
    string PatientName,
    string PatientPhone,
    Guid? DoctorId,
    string Status,
    DateTime CheckInAt,
    DateTime? CalledAt,
    DateTime? StartedAt,
    DateTime? CompletedAt
);
