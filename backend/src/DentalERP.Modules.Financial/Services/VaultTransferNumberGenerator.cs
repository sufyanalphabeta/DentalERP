using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class VaultTransferNumberGenerator(FinancialDbContext db) : IVaultTransferNumberGenerator
{
    public async Task<string> GenerateAsync(int year, CancellationToken ct = default)
    {
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('vault_transfer_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"TRF-{year}-{seq:D6}";
    }
}
