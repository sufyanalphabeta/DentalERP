using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.ToTable("doctor_profiles");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CommissionMethod).HasColumnName("commission_method").HasMaxLength(30).IsRequired();
        builder.Property(x => x.DefaultCommissionValue).HasColumnName("default_commission_value").HasColumnType("numeric(10,2)");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
