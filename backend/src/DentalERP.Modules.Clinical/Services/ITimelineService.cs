namespace DentalERP.Modules.Clinical.Services;

public interface ITimelineService
{
    Task RecordAsync(
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
        CancellationToken ct = default);
}
