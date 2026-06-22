using DentalERP.Modules.Laboratory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Services;

public sealed class LabOrderNumberGenerator(LaboratoryDbContext db) : ILabOrderNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('lab_order_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"LAB-{year}-{seq:D6}";
    }
}
