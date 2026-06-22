using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ToTable("purchase_invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.InvoiceDate).HasColumnName("invoice_date");
        builder.Property(x => x.SupplierId).HasColumnName("supplier_id");
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(12,2)");
        builder.Property(x => x.Discount).HasColumnName("discount").HasColumnType("decimal(12,2)");
        builder.Property(x => x.NetTotal).HasColumnName("net_total").HasColumnType("decimal(12,2)");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedById).HasColumnName("created_by_id");
        builder.Property(x => x.PostedAt).HasColumnName("posted_at");
        builder.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.DomainEvents);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
