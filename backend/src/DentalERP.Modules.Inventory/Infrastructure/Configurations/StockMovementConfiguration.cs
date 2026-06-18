using DentalERP.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Inventory.Infrastructure.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MovementNumber).HasColumnName("movement_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        builder.Property(x => x.BatchId).HasColumnName("batch_id");
        builder.Property(x => x.MovementType).HasColumnName("movement_type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Direction).HasColumnName("direction").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(10,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(12,2)");
        builder.Property(x => x.DestinationType).HasColumnName("destination_type").HasMaxLength(30);
        builder.Property(x => x.DestinationId).HasColumnName("destination_id");
        builder.Property(x => x.ReferenceId).HasColumnName("reference_id");
        builder.Property(x => x.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
        builder.Property(x => x.IsNegativeStock).HasColumnName("is_negative_stock");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedById).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.MovementNumber).IsUnique();
    }
}
