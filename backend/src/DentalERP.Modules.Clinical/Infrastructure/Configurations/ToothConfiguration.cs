using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class ToothConfiguration : IEntityTypeConfiguration<Tooth>
{
    public void Configure(EntityTypeBuilder<Tooth> builder)
    {
        builder.ToTable("teeth");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.FdiNumber).HasColumnName("fdi_number").IsRequired();
        builder.Property(t => t.UniversalNumber).HasColumnName("universal_number");
        builder.Property(t => t.NameAr).HasColumnName("name_ar").HasMaxLength(50).IsRequired();
        builder.Property(t => t.NameEn).HasColumnName("name_en").HasMaxLength(50).IsRequired();
        builder.Property(t => t.Jaw).HasColumnName("jaw").HasMaxLength(10).IsRequired();
        builder.Property(t => t.Side).HasColumnName("side").HasMaxLength(10).IsRequired();
        builder.Property(t => t.ToothType).HasColumnName("tooth_type").HasMaxLength(20).IsRequired();
        builder.Property(t => t.IsPrimary).HasColumnName("is_primary").IsRequired();
        builder.Property(t => t.Position).HasColumnName("position").IsRequired();
        builder.HasIndex(t => t.FdiNumber).IsUnique();
    }
}
