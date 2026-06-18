using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class InsurancePaymentConfiguration : IEntityTypeConfiguration<InsurancePayment>
{
    public void Configure(EntityTypeBuilder<InsurancePayment> builder)
    {
        builder.ToTable("insurance_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InsuranceClaimId).HasColumnName("insurance_claim_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);
        builder.Property(x => x.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(100);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.PaymentDate).HasColumnName("payment_date");
        builder.Property(x => x.ReceivedById).HasColumnName("received_by_id");
    }
}
