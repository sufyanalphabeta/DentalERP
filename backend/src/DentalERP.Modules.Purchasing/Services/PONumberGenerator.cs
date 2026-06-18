using DentalERP.Modules.Purchasing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Services;

public sealed class PONumberGenerator(PurchasingDbContext db) : IPONumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.PurchaseOrders.CountAsync(p => p.CreatedAt.Year == year, ct);
        return $"PO-{year}-{count + 1:D6}";
    }
}

public sealed class GRNumberGenerator(PurchasingDbContext db) : IGRNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.GoodsReceipts.CountAsync(g => g.CreatedAt.Year == year, ct);
        return $"GR-{year}-{count + 1:D6}";
    }
}

public sealed class ReturnNumberGenerator(PurchasingDbContext db) : IReturnNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.PurchaseReturns.CountAsync(r => r.CreatedAt.Year == year, ct);
        return $"PRN-{year}-{count + 1:D6}";
    }
}

public sealed class SupplierPaymentNumberGenerator(PurchasingDbContext db) : ISupplierPaymentNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.SupplierPayments.CountAsync(p => p.CreatedAt.Year == year, ct);
        return $"SPAY-{year}-{count + 1:D6}";
    }
}
