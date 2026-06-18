namespace DentalERP.Modules.Radiology.Domain.Entities;

public sealed class RadiologyImage
{
    public Guid Id { get; private set; }
    public Guid RadiologyOrderId { get; private set; }
    public string StorageBucket { get; private set; } = default!;
    public string StorageKey { get; private set; } = default!;
    public string FileName { get; private set; } = default!;
    public long FileSize { get; private set; }
    public string? ContentType { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedById { get; private set; }

    public RadiologyOrder RadiologyOrder { get; private set; } = default!;

    private RadiologyImage() { }

    public static RadiologyImage Create(Guid orderId, string storageBucket, string storageKey,
        string fileName, long fileSize, string? contentType, Guid uploadedById)
    {
        return new RadiologyImage
        {
            Id = Guid.NewGuid(),
            RadiologyOrderId = orderId,
            StorageBucket = storageBucket,
            StorageKey = storageKey,
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            UploadedById = uploadedById,
            UploadedAt = DateTime.UtcNow
        };
    }
}
