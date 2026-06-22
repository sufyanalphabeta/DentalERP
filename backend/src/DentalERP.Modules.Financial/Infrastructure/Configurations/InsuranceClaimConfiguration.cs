using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
    {
        builder.ToTable("insurance_claims");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClaimNumber).HasColumnName("claim_number").HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.ClaimNumber).IsUnique();
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.HasIndex(x => x.InvoiceId).IsUnique();
        builder.Property(x => x.InsuranceCompanyId).HasColumnName("company_id");
        builder.Property(x => x.PatientId).HasColumnName("patient_id");
        builder.Property(x => x.ClaimedAmount).HasColumnName("claim_amount").HasPrecision(12, 2);
        builder.Property(x => x.CoveragePercent).HasColumnName("coverage_percent").HasPrecision(5, 2);
        builder.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasPrecision(12, 2);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.RejectionReason).HasColumnName("rejection_reason");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.ClaimDate).HasColumnName("submission_date");
        builder.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(x => x.InsuranceCompany)
            .WithMany()
            .HasForeignKey(x => x.InsuranceCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.InsuranceClaim)
            .HasForeignKey(x => x.InsuranceClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Payments).UsePropertyAccessMode(PropertyAccessMode.Field).HasField("_payments");
    }
}
