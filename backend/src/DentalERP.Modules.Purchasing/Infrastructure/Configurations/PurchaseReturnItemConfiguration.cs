using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
    {
        builder.ToTable("purchase_return_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ReturnId).HasColumnName("return_id");
        builder.Property(x => x.ItemId).HasColumnName("item_id");
        builder.Property(x => x.BatchId).HasColumnName("batch_id");
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(10,3)");
        builder.Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10,2)");
        builder.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(12,2)");
    }
}
