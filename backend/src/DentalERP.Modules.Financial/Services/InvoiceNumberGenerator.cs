using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class InvoiceNumberGenerator(FinancialDbContext db) : IInvoiceNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('invoice_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"INV-{year}-{seq:D6}";
    }
}
