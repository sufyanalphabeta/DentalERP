using DentalERP.Modules.Purchasing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Services;

public sealed class PONumberGenerator(PurchasingDbContext db) : IPONumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('po_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"PO-{year}-{seq:D6}";
    }
}

public sealed class GRNumberGenerator(PurchasingDbContext db) : IGRNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('gr_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"GR-{year}-{seq:D6}";
    }
}

public sealed class ReturnNumberGenerator(PurchasingDbContext db) : IReturnNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('purchase_return_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"PRN-{year}-{seq:D6}";
    }
}

public sealed class SupplierPaymentNumberGenerator(PurchasingDbContext db) : ISupplierPaymentNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('supplier_payment_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"SPAY-{year}-{seq:D6}";
    }
}

public sealed class PINumberGenerator(PurchasingDbContext db) : IPINumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('purchase_invoice_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"PI-{year}-{seq:D6}";
    }
}
