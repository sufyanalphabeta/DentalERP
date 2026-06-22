using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class InsuranceClaimNumberGenerator(FinancialDbContext db) : IInsuranceClaimNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('insurance_claim_number_seq') AS \"Value\"")
            .FirstAsync(cancellationToken);
        return $"INS-{year}-{seq:D6}";
    }
}
