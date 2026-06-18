using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class PatientMediaConfiguration : IEntityTypeConfiguration<PatientMedia>
{
    public void Configure(EntityTypeBuilder<PatientMedia> builder)
    {
        builder.ToTable("patient_media");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(m => m.AppointmentId).HasColumnName("appointment_id");
        builder.Property(m => m.MediaType).HasColumnName("media_type").HasMaxLength(20).IsRequired();
        builder.Property(m => m.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(m => m.FilePath).HasColumnName("file_path").IsRequired();
        builder.Property(m => m.FileSizeBytes).HasColumnName("file_size_bytes");
        builder.Property(m => m.MimeType).HasColumnName("mime_type").HasMaxLength(100);
        builder.Property(m => m.ThumbnailPath).HasColumnName("thumbnail_path");
        builder.Property(m => m.Title).HasColumnName("title").HasMaxLength(200);
        builder.Property(m => m.Description).HasColumnName("description");
        builder.Property(m => m.ToothId).HasColumnName("tooth_id");
        builder.Property(m => m.IsRequired).HasColumnName("is_required").IsRequired();
        builder.Property(m => m.IsApproved).HasColumnName("is_approved").IsRequired();
        builder.Property(m => m.ApprovedById).HasColumnName("approved_by_id");
        builder.Property(m => m.ApprovedAt).HasColumnName("approved_at");
        builder.Property(m => m.UploadedById).HasColumnName("uploaded_by_id").IsRequired();
        builder.Property(m => m.UploadedAt).HasColumnName("uploaded_at").IsRequired();
        builder.Property(m => m.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(m => m.DeletedAt == null);

        builder.HasIndex(m => m.PatientId).HasDatabaseName("ix_media_patient");
        builder.HasIndex(m => new { m.PatientId, m.MediaType }).HasDatabaseName("ix_media_type");
        builder.HasIndex(m => m.AppointmentId).HasDatabaseName("ix_media_appt");
    }
}
