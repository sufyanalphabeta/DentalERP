using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.UploadRadiologyImage;

public sealed record UploadRadiologyImageCommand(
    Guid OrderId,
    string StorageBucket,
    string StorageKey,
    string FileName,
    long FileSize,
    string? ContentType,
    Guid UploadedById
) : IRequest<Result<Guid>>;
