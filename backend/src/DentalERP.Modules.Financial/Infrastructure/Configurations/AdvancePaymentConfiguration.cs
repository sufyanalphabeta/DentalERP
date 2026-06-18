using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class AdvancePaymentConfiguration : IEntityTypeConfiguration<AdvancePayment>
{
    public void Configure(EntityTypeBuilder<AdvancePayment> builder)
    {
        builder.ToTable("advance_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.VaultId).HasColumnName("vault_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Remaining).HasColumnName("remaining").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
