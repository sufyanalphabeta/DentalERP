using DentalERP.Modules.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Services;

public sealed class MovementNumberGenerator(InventoryDbContext db) : IMovementNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('stock_movement_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"MOV-{year}-{seq:D6}";
    }
}
