using DentalERP.Modules.Radiology.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Radiology.Infrastructure.Configurations;

public sealed class RadiologyOrderConfiguration : IEntityTypeConfiguration<RadiologyOrder>
{
    public void Configure(EntityTypeBuilder<RadiologyOrder> builder)
    {
        builder.ToTable("radiology_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrderNumber).HasColumnName("order_number").HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.IsExternalPatient).HasColumnName("is_external_patient");
        builder.Property(x => x.ExternalPatientName).HasColumnName("external_patient_name").HasMaxLength(200);
        builder.Property(x => x.ExternalPatientPhone).HasColumnName("external_patient_phone").HasMaxLength(30);
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.TechnicianId).HasColumnName("technician_id");
        builder.Property(x => x.RadiologyTypeId).HasColumnName("radiology_type_id");
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
        builder.Property(x => x.DoctorCommissionAmount).HasColumnName("doctor_commission_amount").HasPrecision(10, 2);
        builder.Property(x => x.TechCommissionAmount).HasColumnName("tech_commission_amount").HasPrecision(10, 2);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CancellationReason).HasColumnName("cancellation_reason");
        builder.Property(x => x.OrderDate).HasColumnName("order_date");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.RadiologyType)
            .WithMany()
            .HasForeignKey(x => x.RadiologyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.RadiologyOrder)
            .HasForeignKey(x => x.RadiologyOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Report)
            .WithOne(x => x.RadiologyOrder)
            .HasForeignKey<RadiologyReport>(x => x.RadiologyOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Images).UsePropertyAccessMode(PropertyAccessMode.Field).HasField("_images");
    }
}
