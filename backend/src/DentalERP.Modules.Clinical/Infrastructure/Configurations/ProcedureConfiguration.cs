using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class ProcedureConfiguration : IEntityTypeConfiguration<Procedure>
{
    public void Configure(EntityTypeBuilder<Procedure> builder)
    {
        builder.ToTable("procedures");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.AppointmentId).HasColumnName("appointment_id").IsRequired();
        builder.Property(p => p.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(p => p.DoctorId).HasColumnName("doctor_id").IsRequired();
        builder.Property(p => p.TreatmentPlanItemId).HasColumnName("treatment_plan_item_id");
        builder.Property(p => p.ToothId).HasColumnName("tooth_id");
        builder.Property(p => p.Surface).HasColumnName("surface").HasMaxLength(20);
        builder.Property(p => p.ProcedureName).HasColumnName("procedure_name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.ProcedureCode).HasColumnName("procedure_code").HasMaxLength(50);
        builder.Property(p => p.ServiceId).HasColumnName("service_id");
        builder.Property(p => p.Notes).HasColumnName("notes");
        builder.Property(p => p.PerformedAt).HasColumnName("performed_at").IsRequired();
        builder.Property(p => p.DurationMinutes).HasColumnName("duration_minutes");
        builder.Property(p => p.BillingStatus).HasColumnName("billing_status").HasMaxLength(20).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(p => p.AppointmentId).HasDatabaseName("ix_procedures_appointment");
        builder.HasIndex(p => p.PatientId).HasDatabaseName("ix_procedures_patient");
        builder.HasIndex(p => p.DoctorId).HasDatabaseName("ix_procedures_doctor");
    }
}
