using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InvoiceId).HasColumnName("invoice_id");
        builder.Property(x => x.ProcedureId).HasColumnName("procedure_id");
        builder.Property(x => x.ServiceName).HasColumnName("service_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ServiceCode).HasColumnName("service_code").HasMaxLength(30);
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Discount).HasColumnName("discount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric(12,2)");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
