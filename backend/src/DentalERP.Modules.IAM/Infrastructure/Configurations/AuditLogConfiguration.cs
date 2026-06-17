using DentalERP.Modules.IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.IAM.Infrastructure.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasColumnName("entity_name").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasColumnName("entity_id").HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(a => a.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(a => a.Timestamp).HasColumnName("timestamp");

        builder.HasIndex(a => a.Timestamp).HasDatabaseName("ix_audit_logs_timestamp");
        builder.HasIndex(a => new { a.EntityName, a.EntityId }).HasDatabaseName("ix_audit_logs_entity");
        builder.HasIndex(a => a.UserId).HasDatabaseName("ix_audit_logs_user");
    }
}
