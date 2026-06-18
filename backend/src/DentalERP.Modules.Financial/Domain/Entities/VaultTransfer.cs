using DentalERP.SharedKernel.Results;

namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class VaultTransfer
{
    public Guid Id { get; private set; }
    public string TransferNumber { get; private set; } = default!;
    public Guid FromVaultId { get; private set; }
    public Guid ToVaultId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public Guid TransferredById { get; private set; }
    public DateTime TransferDate { get; private set; }

    public Vault FromVault { get; private set; } = default!;
    public Vault ToVault { get; private set; } = default!;

    private VaultTransfer() { }

    public static Result<VaultTransfer> Create(string transferNumber, Guid fromVaultId,
        Guid toVaultId, decimal amount, string? notes, Guid transferredById)
    {
        if (fromVaultId == toVaultId)
            return Result.Failure<VaultTransfer>(new Error("VaultTransfer.SameVault", "Source and destination vaults must be different."));
        if (amount <= 0)
            return Result.Failure<VaultTransfer>(new Error("VaultTransfer.InvalidAmount", "Transfer amount must be positive."));

        return Result.Success(new VaultTransfer
        {
            Id = Guid.NewGuid(),
            TransferNumber = transferNumber,
            FromVaultId = fromVaultId,
            ToVaultId = toVaultId,
            Amount = amount,
            Notes = notes,
            TransferredById = transferredById,
            TransferDate = DateTime.UtcNow
        });
    }
}
