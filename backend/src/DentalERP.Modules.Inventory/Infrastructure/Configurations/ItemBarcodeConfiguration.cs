using DentalERP.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Inventory.Infrastructure.Configurations;

public sealed class ItemBarcodeConfiguration : IEntityTypeConfiguration<ItemBarcode>
{
    public void Configure(EntityTypeBuilder<ItemBarcode> builder)
    {
        builder.ToTable("item_barcodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.Barcode).HasColumnName("barcode").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Label).HasColumnName("label").HasMaxLength(100);
        builder.Property(x => x.IsPrimary).HasColumnName("is_primary");
        builder.HasIndex(x => x.Barcode).IsUnique();
    }
}
