using DentalERP.Modules.Laboratory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Laboratory.Infrastructure.Configurations;

public sealed class ExternalLabConfiguration : IEntityTypeConfiguration<ExternalLab>
{
    public void Configure(EntityTypeBuilder<ExternalLab> builder)
    {
        builder.ToTable("external_labs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContactName).HasColumnName("contact_name").HasMaxLength(100);
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(x => x.Address).HasColumnName("address");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
