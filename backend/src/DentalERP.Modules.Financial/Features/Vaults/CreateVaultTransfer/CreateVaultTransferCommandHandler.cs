using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Vaults.CreateVaultTransfer;

public sealed class CreateVaultTransferCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateVaultTransferCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVaultTransferCommand request, CancellationToken cancellationToken)
    {
        var fromVault = await db.Vaults.FirstOrDefaultAsync(v => v.Id == request.FromVaultId, cancellationToken);
        if (fromVault is null)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "Source vault not found."));

        var toVault = await db.Vaults.FirstOrDefaultAsync(v => v.Id == request.ToVaultId, cancellationToken);
        if (toVault is null)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "Destination vault not found."));

        var year = DateTime.UtcNow.Year;
        var count = await db.VaultTransfers.CountAsync(t => t.TransferDate.Year == year, cancellationToken);
        var transferNumber = $"TRF-{year}-{count + 1:D6}";

        var transferResult = VaultTransfer.Create(transferNumber, request.FromVaultId,
            request.ToVaultId, request.Amount, request.Notes, request.TransferredById);

        if (!transferResult.IsSuccess)
            return Result.Failure<Guid>(transferResult.Error!);

        var transfer = transferResult.Value!;
        db.VaultTransfers.Add(transfer);

        // Create the two balancing vault transactions
        var outTx = VaultTransaction.Create(request.FromVaultId, "inter_vault_transfer",
            request.Amount, "out", notes: request.Notes, createdByUserId: request.TransferredById);
        var inTx = VaultTransaction.Create(request.ToVaultId, "inter_vault_transfer",
            request.Amount, "in", notes: request.Notes, createdByUserId: request.TransferredById);

        db.VaultTransactions.Add(outTx);
        db.VaultTransactions.Add(inTx);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(transfer.Id);
    }
}
