using DentalERP.Modules.Purchasing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Services;

public sealed class SupplierCodeGenerator(PurchasingDbContext db) : ISupplierCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var count = await db.Suppliers.IgnoreQueryFilters().CountAsync(ct);
        return $"SUP-{count + 1:D6}";
    }
}
