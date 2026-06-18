using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Media.UploadMedia;

public sealed class UploadMediaCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<UploadMediaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadMediaCommand request, CancellationToken ct)
    {
        if (!PatientMedia.ValidMediaTypes.Contains(request.MediaType))
            return Result.Failure<Guid>(new Error("Media.InvalidType",
                $"Media type '{request.MediaType}' is not valid. Valid: {string.Join(", ", PatientMedia.ValidMediaTypes)}"));

        var media = PatientMedia.Create(
            request.PatientId,
            request.UploadedById,
            request.MediaType,
            request.FileName,
            request.FilePath,
            request.FileSizeBytes,
            request.MimeType,
            request.ThumbnailPath,
            request.Title,
            request.Description,
            request.ToothId,
            request.AppointmentId,
            request.IsRequired);

        db.PatientMedia.Add(media);
        await db.SaveChangesAsync(ct);

        var category = request.MediaType is "OPG" or "CBCT" or "XRay"
            ? PatientTimelineEvent.Categories.Radiology
            : PatientTimelineEvent.Categories.Clinical;

        await timeline.RecordAsync(
            request.PatientId,
            PatientTimelineEvent.EventTypes.MediaUploaded,
            $"رفع {request.MediaType}: {request.Title ?? request.FileName}",
            category,
            actorId: request.UploadedById,
            linkedEntityType: "PatientMedia",
            linkedEntityId: media.Id,
            ct: ct);

        return Result.Success(media.Id);
    }
}
