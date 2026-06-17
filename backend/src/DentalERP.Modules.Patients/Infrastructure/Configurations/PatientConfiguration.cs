using DentalERP.Modules.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Patients.Infrastructure.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.FileNumber).HasColumnName("file_number").HasMaxLength(20).IsRequired();
        builder.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(p => p.Gender).HasColumnName("gender").HasMaxLength(10);
        builder.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        builder.Property(p => p.Phone2).HasColumnName("phone2").HasMaxLength(20);
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(p => p.Address).HasColumnName("address");
        builder.Property(p => p.NationalId).HasColumnName("national_id").HasMaxLength(50);
        builder.Property(p => p.BloodType).HasColumnName("blood_type").HasMaxLength(5);
        builder.Property(p => p.Allergies).HasColumnName("allergies");
        builder.Property(p => p.ChronicDiseases).HasColumnName("chronic_diseases");
        builder.Property(p => p.Notes).HasColumnName("notes");
        builder.Property(p => p.IsActive).HasColumnName("is_active");
        builder.Property(p => p.InsuranceCompanyId).HasColumnName("insurance_company_id");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(p => p.FileNumber).IsUnique();
        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.Ignore(p => p.Age);
        builder.Ignore(p => p.IsDeleted);
        builder.Ignore(p => p.DomainEvents);
    }
}
