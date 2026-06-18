namespace DentalERP.Modules.Laboratory.Domain.Entities;

public sealed class LabResult
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string? ResultNotes { get; private set; }
    public string? StorageBucket { get; private set; }
    public string? StorageKey { get; private set; }
    public string? FileName { get; private set; }
    public long? FileSize { get; private set; }
    public Guid? ReceivedById { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    private LabResult() { }

    public static LabResult Create(Guid orderId, string? resultNotes,
        string? storageBucket = null, string? storageKey = null,
        string? fileName = null, long? fileSize = null,
        Guid? receivedById = null)
        => new()
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ResultNotes = resultNotes,
            StorageBucket = storageBucket,
            StorageKey = storageKey,
            FileName = fileName,
            FileSize = fileSize,
            ReceivedById = receivedById,
            ReceivedAt = DateTime.UtcNow
        };
}
