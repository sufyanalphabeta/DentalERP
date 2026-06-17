using DentalERP.Modules.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Patients.Infrastructure.Configurations;

public sealed class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("queue_entries");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasColumnName("id");
        builder.Property(q => q.AppointmentId).HasColumnName("appointment_id");
        builder.Property(q => q.PatientId).HasColumnName("patient_id");
        builder.Property(q => q.DoctorId).HasColumnName("doctor_id");
        builder.Property(q => q.QueueDate).HasColumnName("queue_date");
        builder.Property(q => q.TokenNumber).HasColumnName("token_number");
        builder.Property(q => q.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.CheckInAt).HasColumnName("check_in_at");
        builder.Property(q => q.CalledAt).HasColumnName("called_at");
        builder.Property(q => q.StartedAt).HasColumnName("started_at");
        builder.Property(q => q.CompletedAt).HasColumnName("completed_at");
        builder.Property(q => q.Notes).HasColumnName("notes");
        builder.Property(q => q.CreatedAt).HasColumnName("created_at");
        builder.Property(q => q.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(q => new { q.QueueDate, q.TokenNumber }).IsUnique();

        builder.HasOne(q => q.Patient)
            .WithMany()
            .HasForeignKey(q => q.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Appointment)
            .WithMany()
            .HasForeignKey(q => q.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(q => q.DeletedAt);
        builder.Ignore(q => q.IsDeleted);
        builder.Ignore(q => q.DomainEvents);
    }
}
