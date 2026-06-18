using DentalERP.Modules.Laboratory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Laboratory.Infrastructure.Configurations;

public sealed class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("lab_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderNumber).HasColumnName("order_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.LabId).HasColumnName("lab_id");
        builder.Property(x => x.ClientId).HasColumnName("client_id");
        builder.Property(x => x.ProcedureId).HasColumnName("procedure_id");
        builder.Property(x => x.IsExternal).HasColumnName("is_external");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.SentAt).HasColumnName("sent_at");
        builder.Property(x => x.ExpectedAt).HasColumnName("expected_at");
        builder.Property(x => x.ReceivedAt).HasColumnName("received_at");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasPrecision(12, 2);
        builder.Property(x => x.TotalRevenue).HasColumnName("total_revenue").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CancelledReason).HasColumnName("cancelled_reason");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Results)
            .WithOne()
            .HasForeignKey(r => r.OrderId);
    }
}
