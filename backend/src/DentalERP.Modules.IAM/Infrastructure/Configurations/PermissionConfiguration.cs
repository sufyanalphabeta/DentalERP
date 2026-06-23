using DentalERP.Modules.IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.IAM.Infrastructure.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Module).HasColumnName("module").HasMaxLength(50).IsRequired();
        builder.Property(p => p.Screen).HasColumnName("screen").HasMaxLength(100);
        builder.Property(p => p.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Ignore(p => p.Action);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(p => p.Name).IsUnique().HasDatabaseName("ux_permissions_name");
        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
