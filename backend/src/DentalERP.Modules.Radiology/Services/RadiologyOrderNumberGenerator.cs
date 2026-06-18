using DentalERP.Modules.Radiology.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Services;

public sealed class RadiologyOrderNumberGenerator(RadiologyDbContext db) : IRadiologyOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.RadiologyOrders
            .CountAsync(o => o.OrderDate.Year == year, cancellationToken);
        return $"RAD-{year}-{count + 1:D6}";
    }
}
