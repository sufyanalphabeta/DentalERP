using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class InstallmentPaymentConfiguration : IEntityTypeConfiguration<InstallmentPayment>
{
    public void Configure(EntityTypeBuilder<InstallmentPayment> builder)
    {
        builder.ToTable("installment_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlanId).HasColumnName("plan_id");
        builder.Property(x => x.InstallmentNum).HasColumnName("installment_num");
        builder.Property(x => x.DueDate).HasColumnName("due_date");
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.PaidAt).HasColumnName("paid_at");
        builder.Property(x => x.VaultId).HasColumnName("vault_id");
        builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20);
    }
}
