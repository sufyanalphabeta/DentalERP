using DentalERP.Modules.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalERP.Modules.Financial.Infrastructure.Configurations;

public sealed class VaultTransferConfiguration : IEntityTypeConfiguration<VaultTransfer>
{
    public void Configure(EntityTypeBuilder<VaultTransfer> builder)
    {
        builder.ToTable("vault_transfers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TransferNumber).HasColumnName("transfer_number").HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.TransferNumber).IsUnique();
        builder.Property(x => x.FromVaultId).HasColumnName("from_vault_id");
        builder.Property(x => x.ToVaultId).HasColumnName("to_vault_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(10, 2);
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.TransferredById).HasColumnName("transferred_by_id");
        builder.Property(x => x.TransferDate).HasColumnName("transfer_date");

        builder.HasOne(x => x.FromVault)
            .WithMany()
            .HasForeignKey(x => x.FromVaultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToVault)
            .WithMany()
            .HasForeignKey(x => x.ToVaultId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
