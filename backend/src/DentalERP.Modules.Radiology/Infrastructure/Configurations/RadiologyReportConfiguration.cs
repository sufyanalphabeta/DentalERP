using DentalERP.Modules.Radiology.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Radiology.Infrastructure.Configurations;

public sealed class RadiologyReportConfiguration : IEntityTypeConfiguration<RadiologyReport>
{
    public void Configure(EntityTypeBuilder<RadiologyReport> builder)
    {
        builder.ToTable("radiology_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RadiologyOrderId).HasColumnName("radiology_order_id");
        builder.Property(x => x.ReportText).HasColumnName("report_text").IsRequired();
        builder.Property(x => x.ReportedById).HasColumnName("reported_by_id");
        builder.Property(x => x.ReportedAt).HasColumnName("reported_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.RadiologyOrderId).IsUnique();
    }
}
