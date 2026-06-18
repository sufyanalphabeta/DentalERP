using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSuppliers;

public sealed record GetSuppliersQuery(
    string? Search, bool? ActiveOnly, string? Category, int Page = 1, int PageSize = 20)
    : IRequest<Result<GetSuppliersResult>>;

public sealed record GetSuppliersResult(IReadOnlyList<SupplierListItem> Suppliers, int Total);

public sealed record SupplierListItem(
    Guid Id, string SupplierCode, string Name, string? NameAr,
    string? Category, string? Phone, string? Email, bool IsActive,
    decimal ComputedBalance);

public sealed class GetSuppliersQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSuppliersQuery, Result<GetSuppliersResult>>
{
    public async Task<Result<GetSuppliersResult>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Suppliers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(s => s.Name.Contains(request.Search) || s.SupplierCode.Contains(request.Search));
        if (request.ActiveOnly == true)
            query = query.Where(s => s.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(s => s.Category == request.Category);

        var total = await query.CountAsync(cancellationToken);
        var suppliers = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var ids = suppliers.Select(s => s.Id).ToList();

        // Compute balances: GRs - Payments - Confirmed Returns
        var grTotals = await db.GoodsReceipts
            .Where(g => ids.Contains(g.SupplierId))
            .GroupBy(g => g.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(g => g.SupplierId, g => g.Total, cancellationToken);

        var paymentTotals = await db.SupplierPayments
            .Where(p => ids.Contains(p.SupplierId))
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(g => g.SupplierId, g => g.Total, cancellationToken);

        var returnTotals = await db.PurchaseReturns
            .Where(r => ids.Contains(r.SupplierId) && r.Status == "Confirmed")
            .GroupBy(r => r.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToDictionaryAsync(g => g.SupplierId, g => g.Total, cancellationToken);

        var result = suppliers.Select(s =>
        {
            var balance = grTotals.GetValueOrDefault(s.Id, 0)
                        - paymentTotals.GetValueOrDefault(s.Id, 0)
                        - returnTotals.GetValueOrDefault(s.Id, 0);
            return new SupplierListItem(s.Id, s.SupplierCode, s.Name, s.NameAr, s.Category, s.Phone, s.Email, s.IsActive, balance);
        }).ToList();

        return Result.Success(new GetSuppliersResult(result, total));
    }
}
