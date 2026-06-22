using DentalERP.Modules.Radiology.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Services;

public sealed class RadiologyOrderNumberGenerator(RadiologyDbContext db) : IRadiologyOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('radiology_order_number_seq') AS \"Value\"")
            .FirstAsync(cancellationToken);
        return $"RAD-{year}-{seq:D6}";
    }
}
