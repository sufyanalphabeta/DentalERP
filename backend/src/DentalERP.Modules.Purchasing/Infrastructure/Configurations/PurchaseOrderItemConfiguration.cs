using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PoId).HasColumnName("po_id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.SupplierItemId).HasColumnName("supplier_item_id");
        builder.Property(x => x.QuantityOrdered).HasColumnName("quantity_ordered").HasColumnType("decimal(10,3)");
        builder.Property(x => x.QuantityReceived).HasColumnName("quantity_received").HasColumnType("decimal(10,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(12,2)");
        builder.Property(x => x.Notes).HasColumnName("notes");
    }
}
