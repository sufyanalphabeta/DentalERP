using System.Text.Json;
using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;

namespace DentalERP.Modules.Clinical.Services;

public sealed class TimelineService(ClinicalDbContext db) : ITimelineService
{
    public async Task RecordAsync(
        Guid patientId,
        string eventType,
        string title,
        string eventCategory = "Administrative",
        string? description = null,
        Guid? actorId = null,
        string? actorName = null,
        string? linkedEntityType = null,
        Guid? linkedEntityId = null,
        object? metadata = null,
        bool isVisibleToDoctor = true,
        bool isVisibleToPatient = false,
        CancellationToken ct = default)
    {
        var metadataJson = metadata is null
            ? null
            : JsonSerializer.Serialize(metadata);

        var evt = PatientTimelineEvent.Create(
            patientId,
            eventType,
            title,
            eventCategory,
            description,
            actorId,
            actorName,
            linkedEntityType,
            linkedEntityId,
            metadataJson,
            isVisibleToDoctor,
            isVisibleToPatient);

        db.PatientTimeline.Add(evt);
        await db.SaveChangesAsync(ct);
    }
}
