using DentalERP.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Inventory.Infrastructure.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ItemCode).HasColumnName("item_code").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Barcode).HasColumnName("barcode").HasMaxLength(100);
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(x => x.CategoryId).HasColumnName("category_id");
        builder.Property(x => x.UnitOfMeasureId).HasColumnName("unit_of_measure_id");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.SalePrice).HasColumnName("sale_price").HasColumnType("decimal(10,2)");
        builder.Property(x => x.ReorderLevel).HasColumnName("reorder_level").HasColumnType("decimal(10,3)");
        builder.Property(x => x.ReorderQuantity).HasColumnName("reorder_quantity").HasColumnType("decimal(10,3)");
        builder.Property(x => x.IsExpiryTracked).HasColumnName("is_expiry_tracked");
        builder.Property(x => x.AllowNegativeStock).HasColumnName("allow_negative_stock");
        builder.Property(x => x.StorageConditions).HasColumnName("storage_conditions").HasMaxLength(200);
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.DomainEvents);

        builder.HasMany(x => x.Barcodes)
            .WithOne()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
