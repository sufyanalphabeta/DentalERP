using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("goods_receipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.GrNumber).HasColumnName("gr_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PoId).HasColumnName("po_id");
        builder.Property(x => x.SupplierId).HasColumnName("supplier_id");
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        builder.Property(x => x.ReceiptDate).HasColumnName("receipt_date");
        builder.Property(x => x.SupplierInvoiceRef).HasColumnName("supplier_invoice_ref").HasMaxLength(100);
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(12,2)");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.ReceivedById).HasColumnName("received_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.GrNumber).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.GrId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
