using DentalERP.Modules.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Inventory.Infrastructure.Configurations;

public sealed class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> builder)
    {
        builder.ToTable("stock_batches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.WarehouseId).HasColumnName("warehouse_id");
        builder.Property(x => x.BatchNumber).HasColumnName("batch_number").HasMaxLength(100);
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(10,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(x => x.ReceivedDate).HasColumnName("received_date");
        builder.Property(x => x.IsDepleted).HasColumnName("is_depleted");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
