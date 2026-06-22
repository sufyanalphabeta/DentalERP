using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseInvoices;

public sealed record GetPurchaseInvoicesQuery(
    Guid? SupplierId, string? Status, string? Search,
    DateOnly? From, DateOnly? To,
    int Page = 1, int PageSize = 20) : IRequest<Result<GetPurchaseInvoicesResult>>;

public sealed record GetPurchaseInvoicesResult(IReadOnlyList<PIListItem> Invoices, int Total);

public sealed record PIListItem(
    Guid Id, string InvoiceNumber, DateOnly InvoiceDate,
    Guid SupplierId, string SupplierName,
    string Status, decimal NetTotal, int ItemCount,
    DateTime CreatedAt, DateTime? PostedAt);

public sealed class GetPurchaseInvoicesQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchaseInvoicesQuery, Result<GetPurchaseInvoicesResult>>
{
    public async Task<Result<GetPurchaseInvoicesResult>> Handle(
        GetPurchaseInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = db.PurchaseInvoices.AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (request.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (request.From.HasValue)
            query = query.Where(x => x.InvoiceDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(x => x.InvoiceDate <= request.To.Value);
        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(x => x.InvoiceNumber.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        var invoices = await query
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var supplierIds = invoices.Select(i => i.SupplierId).Distinct().ToList();
        var suppliers = await db.Suppliers.IgnoreQueryFilters()
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var itemCounts = await db.PurchaseInvoiceItems
            .Where(x => invoiceIds.Contains(x.InvoiceId))
            .GroupBy(x => x.InvoiceId)
            .Select(g => new { InvoiceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.InvoiceId, g => g.Count, cancellationToken);

        var result = invoices.Select(inv => new PIListItem(
            inv.Id, inv.InvoiceNumber, inv.InvoiceDate,
            inv.SupplierId, suppliers.GetValueOrDefault(inv.SupplierId, "—"),
            inv.Status, inv.NetTotal,
            itemCounts.GetValueOrDefault(inv.Id, 0),
            inv.CreatedAt, inv.PostedAt)).ToList();

        return Result.Success(new GetPurchaseInvoicesResult(result, total));
    }
}
