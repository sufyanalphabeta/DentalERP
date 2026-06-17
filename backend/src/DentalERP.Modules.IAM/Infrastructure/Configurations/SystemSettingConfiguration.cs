using DentalERP.Modules.IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.IAM.Infrastructure.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("system_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Value).HasColumnName("value").HasMaxLength(2000).IsRequired();
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(s => s.Group).HasColumnName("group").HasMaxLength(50).HasDefaultValue("General");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(s => s.Key).IsUnique().HasDatabaseName("ux_system_settings_key");
        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}
