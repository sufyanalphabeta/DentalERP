using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.RecordLabResult;

public sealed record RecordLabResultCommand(
    Guid OrderId,
    string? ResultNotes,
    string? StorageBucket,
    string? StorageKey,
    string? FileName,
    long? FileSize,
    Guid? ReceivedById = null
) : IRequest<Result>;
