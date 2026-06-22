using DentalERP.Modules.Radiology.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Radiology.Infrastructure.Configurations;

public sealed class RadiologyImageConfiguration : IEntityTypeConfiguration<RadiologyImage>
{
    public void Configure(EntityTypeBuilder<RadiologyImage> builder)
    {
        builder.ToTable("radiology_images");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RadiologyOrderId).HasColumnName("order_id");
        builder.Property(x => x.StorageBucket).HasColumnName("storage_bucket").HasMaxLength(200).IsRequired();
        builder.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(500).IsRequired();
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(300).IsRequired();
        builder.Property(x => x.FileSize).HasColumnName("file_size");
        builder.Property(x => x.ContentType).HasColumnName("mime_type").HasMaxLength(100);
        builder.Property(x => x.UploadedAt).HasColumnName("taken_at");
        builder.Property(x => x.UploadedById).HasColumnName("uploaded_by_id");
    }
}
