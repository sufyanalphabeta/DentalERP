using DentalERP.Modules.Purchasing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Purchasing.Infrastructure.Configurations;

public sealed class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.ToTable("supplier_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PaymentNumber).HasColumnName("payment_number").HasMaxLength(30).IsRequired();
        builder.Property(x => x.SupplierId).HasColumnName("supplier_id");
        builder.Property(x => x.VaultId).HasColumnName("vault_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
        builder.Property(x => x.PaymentDate).HasColumnName("payment_date");
        builder.Property(x => x.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(100);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.PaidById).HasColumnName("paid_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.PaymentNumber).IsUnique();
    }
}
