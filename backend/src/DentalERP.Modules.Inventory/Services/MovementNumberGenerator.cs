using DentalERP.Modules.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Services;

public sealed class MovementNumberGenerator(InventoryDbContext db) : IMovementNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.StockMovements
            .CountAsync(m => m.CreatedAt.Year == year, ct);
        return $"MOV-{year}-{count + 1:D6}";
    }
}
