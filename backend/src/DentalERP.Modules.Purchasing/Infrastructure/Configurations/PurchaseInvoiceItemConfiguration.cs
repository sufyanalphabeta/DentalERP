using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
    {
        builder.ToTable("purchase_invoice_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(50);
        builder.Property(x => x.ItemName).HasColumnName("item_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Barcode).HasColumnName("barcode").HasMaxLength(100);
        builder.Property(x => x.UnitName).HasColumnName("unit_name").HasMaxLength(50);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(12,3)");
        builder.Property(x => x.PurchasePrice).HasColumnName("purchase_price").HasColumnType("decimal(12,2)");
        builder.Property(x => x.SalePrice).HasColumnName("sale_price").HasColumnType("decimal(12,2)");
        builder.Property(x => x.LineTotal).HasColumnName("line_total").HasColumnType("decimal(12,2)");
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(x => x.BatchNumber).HasColumnName("batch_number").HasMaxLength(50);
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
