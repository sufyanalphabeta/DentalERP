using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class DentalChartEntryConfiguration : IEntityTypeConfiguration<DentalChartEntry>
{
    public void Configure(EntityTypeBuilder<DentalChartEntry> builder)
    {
        builder.ToTable("dental_chart_entries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(e => e.ToothId).HasColumnName("tooth_id").IsRequired();
        builder.Property(e => e.Surface).HasColumnName("surface").HasMaxLength(20);
        builder.Property(e => e.Condition).HasColumnName("condition").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(10);
        builder.Property(e => e.Notes).HasColumnName("notes");
        builder.Property(e => e.RecordedById).HasColumnName("recorded_by_id").IsRequired();
        builder.Property(e => e.RecordedAt).HasColumnName("recorded_at").IsRequired();
        builder.Property(e => e.AppointmentId).HasColumnName("appointment_id");
        builder.Property(e => e.IsCurrent).HasColumnName("is_current").IsRequired();

        builder.HasOne(e => e.Tooth)
            .WithMany()
            .HasForeignKey(e => e.ToothId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.PatientId, e.ToothId, e.IsCurrent })
            .HasDatabaseName("ix_chart_tooth");
        builder.HasIndex(e => new { e.PatientId, e.RecordedAt })
            .HasDatabaseName("ix_chart_patient_history");
        builder.HasIndex(e => e.AppointmentId)
            .HasDatabaseName("ix_chart_appointment");
    }
}
