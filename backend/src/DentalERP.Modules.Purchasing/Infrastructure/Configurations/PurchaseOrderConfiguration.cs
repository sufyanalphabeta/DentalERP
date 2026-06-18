using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PoNumber).HasColumnName("po_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.SupplierId).HasColumnName("supplier_id");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrderDate).HasColumnName("order_date");
        builder.Property(x => x.ExpectedDate).HasColumnName("expected_date");
        builder.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(12,2)");
        builder.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(12,2)");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(12,2)");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.ApprovedById).HasColumnName("approved_by_id");
        builder.Property(x => x.ApprovedAt).HasColumnName("approved_at");
        builder.Property(x => x.CreatedById).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.PoNumber).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.PoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
