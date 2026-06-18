using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class SupplierItemConfiguration : IEntityTypeConfiguration<SupplierItem>
{
    public void Configure(EntityTypeBuilder<SupplierItem> builder)
    {
        builder.ToTable("supplier_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SupplierId).HasColumnName("supplier_id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.SupplierItemCode).HasColumnName("supplier_item_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SupplierItemName).HasColumnName("supplier_item_name").HasMaxLength(200);
        builder.Property(x => x.LastUnitCost).HasColumnName("last_unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.IsPreferred).HasColumnName("is_preferred");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.HasIndex(x => new { x.SupplierId, x.ItemId }).IsUnique();
        builder.HasIndex(x => new { x.SupplierId, x.SupplierItemCode }).IsUnique();
    }
}
