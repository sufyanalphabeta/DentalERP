using DentalERP.Modules.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Patients.Infrastructure.Configurations;

public sealed class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
{
    public void Configure(EntityTypeBuilder<AppointmentType> builder)
    {
        builder.ToTable("appointment_types");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(t => t.NameAr).HasColumnName("name_ar").HasMaxLength(100).IsRequired();
        builder.Property(t => t.DefaultDurationMinutes).HasColumnName("default_duration_minutes");
        builder.Property(t => t.Color).HasColumnName("color").HasMaxLength(7);
        builder.Property(t => t.IsActive).HasColumnName("is_active");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");

        builder.Ignore(t => t.UpdatedAt);
        builder.Ignore(t => t.DeletedAt);
        builder.Ignore(t => t.IsDeleted);
        builder.Ignore(t => t.DomainEvents);
    }
}
