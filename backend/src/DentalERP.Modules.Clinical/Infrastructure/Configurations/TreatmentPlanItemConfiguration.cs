using DentalERP.Modules.Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Clinical.Infrastructure.Configurations;

public sealed class TreatmentPlanItemConfiguration : IEntityTypeConfiguration<TreatmentPlanItem>
{
    public void Configure(EntityTypeBuilder<TreatmentPlanItem> builder)
    {
        builder.ToTable("treatment_plan_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.TreatmentPlanId).HasColumnName("treatment_plan_id").IsRequired();
        builder.Property(i => i.ToothId).HasColumnName("tooth_id");
        builder.Property(i => i.Surface).HasColumnName("surface").HasMaxLength(20);
        builder.Property(i => i.ProcedureName).HasColumnName("procedure_name").HasMaxLength(200).IsRequired();
        builder.Property(i => i.ProcedureCode).HasColumnName("procedure_code").HasMaxLength(50);
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(i => i.DiscountPercent).HasColumnName("discount_percent").HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(i => i.SequenceNumber).HasColumnName("sequence_number").IsRequired();
        builder.Property(i => i.Notes).HasColumnName("notes");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();

        // TotalPrice is computed in C# — not stored in DB
        builder.Ignore(i => i.TotalPrice);

        builder.HasIndex(i => i.TreatmentPlanId).HasDatabaseName("ix_plan_items_plan");
    }
}
