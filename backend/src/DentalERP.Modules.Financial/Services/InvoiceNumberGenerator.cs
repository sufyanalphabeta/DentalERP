using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class InvoiceNumberGenerator(FinancialDbContext db) : IInvoiceNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Invoices
            .IgnoreQueryFilters()
            .CountAsync(i => i.CreatedAt.Year == year, ct);
        return $"INV-{year}-{(count + 1):D6}";
    }
}
