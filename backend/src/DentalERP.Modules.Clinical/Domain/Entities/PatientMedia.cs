namespace DentalERP.Modules.Clinical.Domain.Entities;

public sealed class PatientMedia
{
    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public string MediaType { get; private set; } = string.Empty; // Before|After|OPG|CBCT|XRay|Document
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;  // MinIO object key
    public long? FileSizeBytes { get; private set; }
    public string? MimeType { get; private set; }
    public string? ThumbnailPath { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public short? ToothId { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsApproved { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid UploadedById { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private PatientMedia() { }

    public static PatientMedia Create(
        Guid patientId,
        Guid uploadedById,
        string mediaType,
        string fileName,
        string filePath,
        long? fileSizeBytes = null,
        string? mimeType = null,
        string? thumbnailPath = null,
        string? title = null,
        string? description = null,
        short? toothId = null,
        Guid? appointmentId = null,
        bool isRequired = false)
    {
        return new PatientMedia
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            UploadedById = uploadedById,
            MediaType = mediaType,
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            ThumbnailPath = thumbnailPath,
            Title = title,
            Description = description,
            ToothId = toothId,
            AppointmentId = appointmentId,
            IsRequired = isRequired,
            IsApproved = false,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid approvedById)
    {
        IsApproved = true;
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;
    }

    public void SoftDelete() => DeletedAt = DateTime.UtcNow;

    public static readonly string[] ValidMediaTypes = ["Before", "After", "OPG", "CBCT", "XRay", "Document"];
}
