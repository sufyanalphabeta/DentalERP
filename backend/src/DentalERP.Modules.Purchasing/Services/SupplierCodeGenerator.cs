using DentalERP.Modules.Purchasing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Services;

public sealed class SupplierCodeGenerator(PurchasingDbContext db) : ISupplierCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('supplier_code_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"SUP-{seq:D6}";
    }
}
