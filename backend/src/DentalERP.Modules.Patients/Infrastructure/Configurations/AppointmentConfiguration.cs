using DentalERP.Modules.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Patients.Infrastructure.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id");
        builder.Property(a => a.DoctorId).HasColumnName("doctor_id");
        builder.Property(a => a.AppointmentTypeId).HasColumnName("appointment_type_id");
        builder.Property(a => a.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(a => a.DurationMinutes).HasColumnName("duration_minutes");
        builder.Property(a => a.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ChiefComplaint).HasColumnName("chief_complaint");
        builder.Property(a => a.Notes).HasColumnName("notes");
        builder.Property(a => a.CancellationReason).HasColumnName("cancellation_reason");
        builder.Property(a => a.CreatedById).HasColumnName("created_by_id");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AppointmentType)
            .WithMany()
            .HasForeignKey(a => a.AppointmentTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(a => a.DeletedAt == null);

        builder.Ignore(a => a.EndsAt);
        builder.Ignore(a => a.IsDeleted);
        builder.Ignore(a => a.DomainEvents);
    }
}
