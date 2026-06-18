using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class InsuranceClaimNumberGenerator(FinancialDbContext db) : IInsuranceClaimNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.InsuranceClaims
            .CountAsync(c => c.ClaimDate.Year == year, cancellationToken);
        return $"INS-{year}-{count + 1:D6}";
    }
}
