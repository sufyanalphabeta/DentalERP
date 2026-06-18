using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class GoodsReceiptItemConfiguration : IEntityTypeConfiguration<GoodsReceiptItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptItem> builder)
    {
        builder.ToTable("goods_receipt_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.GrId).HasColumnName("gr_id");
        builder.Property(x => x.PoItemId).HasColumnName("po_item_id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.BatchNumber).HasColumnName("batch_number").HasMaxLength(100);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(10,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(12,2)");
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
    }
}
