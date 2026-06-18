using DentalERP.Modules.Laboratory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Services;

public sealed class LabOrderNumberGenerator(LaboratoryDbContext db) : ILabOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.LabOrders
            .IgnoreQueryFilters()
            .CountAsync(o => o.CreatedAt.Year == year, ct);
        return $"LAB-{year}-{count + 1:D6}";
    }
}
