using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class TreatmentPlanConfiguration : IEntityTypeConfiguration<TreatmentPlan>
{
    public void Configure(EntityTypeBuilder<TreatmentPlan> builder)
    {
        builder.ToTable("treatment_plans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(p => p.DoctorId).HasColumnName("doctor_id").IsRequired();
        builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.EstimatedCost).HasColumnName("estimated_cost").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.ActualCost).HasColumnName("actual_cost").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.PaidAmount).HasColumnName("paid_amount").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.Priority).HasColumnName("priority").HasMaxLength(10).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(p => p.StartDate).HasColumnName("start_date");
        builder.Property(p => p.EndDate).HasColumnName("end_date");
        builder.Property(p => p.Notes).HasColumnName("notes");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.TreatmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.HasIndex(p => p.PatientId).HasDatabaseName("ix_treatment_patient");
        builder.HasIndex(p => p.DoctorId).HasDatabaseName("ix_treatment_doctor");
    }
}
