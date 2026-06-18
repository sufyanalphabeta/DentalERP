using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Treasury.GetVaultBalances;

public sealed class GetVaultBalancesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetVaultBalancesQuery, Result<List<VaultBalanceDto>>>
{
    public async Task<Result<List<VaultBalanceDto>>> Handle(GetVaultBalancesQuery request, CancellationToken cancellationToken)
    {
        var vaults = await db.Vaults.Where(v => v.IsActive).ToListAsync(cancellationToken);

        var result = new List<VaultBalanceDto>();
        foreach (var vault in vaults)
        {
            var totalIn = await db.VaultTransactions
                .Where(t => t.VaultId == vault.Id && t.Direction == "in" && !t.IsReversed)
                .SumAsync(t => t.Amount, cancellationToken);

            var totalOut = await db.VaultTransactions
                .Where(t => t.VaultId == vault.Id && t.Direction == "out" && !t.IsReversed)
                .SumAsync(t => t.Amount, cancellationToken);

            result.Add(new VaultBalanceDto(
                vault.Id,
                vault.Name,
                vault.Type,
                vault.OpeningBalance,
                totalIn,
                totalOut,
                vault.OpeningBalance + totalIn - totalOut));
        }

        return Result.Success(result);
    }
}
