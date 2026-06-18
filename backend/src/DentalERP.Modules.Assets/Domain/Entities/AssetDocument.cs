using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Assets.Domain.Entities;

public sealed class AssetDocument : BaseEntity
{
    public Guid AssetId { get; private set; }
    public string DocumentType { get; private set; } = "Other";
    public string FileName { get; private set; } = string.Empty;
    public string FileKey { get; private set; } = string.Empty;
    public long? FileSize { get; private set; }
    public string? ContentType { get; private set; }
    public string? Notes { get; private set; }
    public Guid? UploadedById { get; private set; }

    private AssetDocument() { }

    public static AssetDocument Create(Guid assetId, string fileName,
        string fileKey, string? contentType = null, long? fileSize = null,
        string documentType = "Other", string? notes = null, Guid? uploadedById = null)
    {
        return new AssetDocument
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            DocumentType = documentType,
            FileName = fileName.Trim(),
            FileKey = fileKey,
            ContentType = contentType,
            FileSize = fileSize,
            Notes = notes,
            UploadedById = uploadedById,
            CreatedAt = DateTime.UtcNow
        };
    }
}
