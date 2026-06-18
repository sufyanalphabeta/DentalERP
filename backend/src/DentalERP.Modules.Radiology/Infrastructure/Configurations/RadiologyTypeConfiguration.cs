using DentalERP.Modules.Radiology.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Radiology.Infrastructure.Configurations;

public sealed class RadiologyTypeConfiguration : IEntityTypeConfiguration<RadiologyType>
{
    public void Configure(EntityTypeBuilder<RadiologyType> builder)
    {
        builder.ToTable("radiology_types");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(x => x.BasePrice).HasColumnName("base_price").HasPrecision(10, 2);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
