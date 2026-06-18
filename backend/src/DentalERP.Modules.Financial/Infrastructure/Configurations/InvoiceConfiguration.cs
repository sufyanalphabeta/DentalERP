using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(12,2)");
        builder.Property(x => x.DiscountTotal).HasColumnName("discount_total").HasColumnType("numeric(12,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CancelledReason).HasColumnName("cancelled_reason");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.Remaining);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasQueryFilter(x => x.DeletedAt == null);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(i => i.InvoiceId);
    }
}
