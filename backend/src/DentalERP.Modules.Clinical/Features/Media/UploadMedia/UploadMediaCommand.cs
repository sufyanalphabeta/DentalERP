using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Media.UploadMedia;

public sealed record UploadMediaCommand(
    Guid PatientId,
    Guid UploadedById,
    string MediaType,
    string FileName,
    string FilePath,
    long? FileSizeBytes = null,
    string? MimeType = null,
    string? ThumbnailPath = null,
    string? Title = null,
    string? Description = null,
    short? ToothId = null,
    Guid? AppointmentId = null,
    bool IsRequired = false) : IRequest<Result<Guid>>;
