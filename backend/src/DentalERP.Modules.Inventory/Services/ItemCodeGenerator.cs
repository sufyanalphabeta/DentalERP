using DentalERP.Modules.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Services;

public sealed class ItemCodeGenerator(InventoryDbContext db) : IItemCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('item_code_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"ITM-{seq:D6}";
    }
}
