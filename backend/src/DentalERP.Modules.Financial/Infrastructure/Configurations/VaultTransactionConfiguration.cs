using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class VaultTransactionConfiguration : IEntityTypeConfiguration<VaultTransaction>
{
    public void Configure(EntityTypeBuilder<VaultTransaction> builder)
    {
        builder.ToTable("vault_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.VaultId).HasColumnName("vault_id");
        builder.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Direction).HasColumnName("direction").HasMaxLength(3).IsRequired();
        builder.Property(x => x.RelatedInvoiceId).HasColumnName("related_invoice_id");
        builder.Property(x => x.RelatedPatientId).HasColumnName("related_patient_id");
        builder.Property(x => x.RelatedDoctorId).HasColumnName("related_doctor_id");
        builder.Property(x => x.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(50);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.IsReversed).HasColumnName("is_reversed");
        builder.Property(x => x.IsReversal).HasColumnName("is_reversal");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
