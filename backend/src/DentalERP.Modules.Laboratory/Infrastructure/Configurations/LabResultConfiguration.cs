using DentalERP.Modules.Laboratory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Laboratory.Infrastructure.Configurations;

public sealed class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.ToTable("lab_results");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.ResultNotes).HasColumnName("result_notes");
        builder.Property(x => x.StorageBucket).HasColumnName("storage_bucket").HasMaxLength(100);
        builder.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(500);
        builder.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(200);
        builder.Property(x => x.FileSize).HasColumnName("file_size");
        builder.Property(x => x.ReceivedById).HasColumnName("received_by_id");
        builder.Property(x => x.ReceivedAt).HasColumnName("received_at");
    }
}
