using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class DoctorAssignmentConfiguration : IEntityTypeConfiguration<DoctorAssignment>
{
    public void Configure(EntityTypeBuilder<DoctorAssignment> builder)
    {
        builder.ToTable("doctor_assignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.PatientId).HasColumnName("patient_id").IsRequired();
        builder.Property(a => a.DoctorId).HasColumnName("doctor_id").IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(a => a.AssignedAt).HasColumnName("assigned_at").IsRequired();
        builder.Property(a => a.EndedAt).HasColumnName("ended_at");
        builder.Property(a => a.CanView).HasColumnName("can_view").IsRequired();
        builder.Property(a => a.CanEdit).HasColumnName("can_edit").IsRequired();
        builder.Property(a => a.TransferredToId).HasColumnName("transferred_to_id");
        builder.Property(a => a.TransferredAt).HasColumnName("transferred_at");
        builder.Property(a => a.TransferReason).HasColumnName("transfer_reason");
        builder.Property(a => a.IsPrimary).HasColumnName("is_primary").IsRequired();
        builder.Property(a => a.Notes).HasColumnName("notes");
        builder.Property(a => a.AssignedById).HasColumnName("assigned_by_id");

        // No UNIQUE constraint — same doctor can be re-assigned after Completed/Transferred
        builder.HasIndex(a => a.PatientId).HasDatabaseName("ix_assignment_patient");
        builder.HasIndex(a => a.DoctorId).HasDatabaseName("ix_assignment_doctor");
    }
}
