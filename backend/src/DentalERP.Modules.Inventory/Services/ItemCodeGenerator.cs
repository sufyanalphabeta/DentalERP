using DentalERP.Modules.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Services;

public sealed class ItemCodeGenerator(InventoryDbContext db) : IItemCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var count = await db.Items.IgnoreQueryFilters().CountAsync(ct);
        return $"ITM-{count + 1:D6}";
    }
}
