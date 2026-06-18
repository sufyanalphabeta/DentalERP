using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class CommissionRecordConfiguration : IEntityTypeConfiguration<CommissionRecord>
{
    public void Configure(EntityTypeBuilder<CommissionRecord> builder)
    {
        builder.ToTable("commission_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DoctorId).HasColumnName("doctor_id");
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.ProcedureId).HasColumnName("procedure_id");
        builder.Property(x => x.CommissionMethod).HasColumnName("commission_method").HasMaxLength(30).IsRequired();
        builder.Property(x => x.BaseAmount).HasColumnName("base_amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.CommissionRate).HasColumnName("commission_rate").HasColumnType("numeric(10,4)");
        builder.Property(x => x.CommissionAmount).HasColumnName("commission_amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.IsPaid).HasColumnName("is_paid");
        builder.Property(x => x.PaidAt).HasColumnName("paid_at");
        builder.Property(x => x.VaultTransactionId).HasColumnName("vault_transaction_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.DoctorId, x.IsPaid });
    }
}
