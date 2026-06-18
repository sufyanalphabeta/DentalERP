using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseReturns;

public sealed record GetPurchaseReturnsQuery(
    Guid? SupplierId, string? Status, int Page = 1, int PageSize = 20)
    : IRequest<Result<GetReturnsResult>>;

public sealed record GetReturnsResult(IReadOnlyList<ReturnListItem> Returns, int Total);

public sealed record ReturnListItem(
    Guid Id, string ReturnNumber, string SupplierName, string Status,
    DateOnly ReturnDate, decimal TotalAmount, string Reason, DateTime CreatedAt);

public sealed class GetPurchaseReturnsQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchaseReturnsQuery, Result<GetReturnsResult>>
{
    public async Task<Result<GetReturnsResult>> Handle(GetPurchaseReturnsQuery request, CancellationToken cancellationToken)
    {
        var query = db.PurchaseReturns.AsNoTracking();

        if (request.SupplierId.HasValue) query = query.Where(r => r.SupplierId == request.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(r => r.Status == request.Status);

        var total = await query.CountAsync(cancellationToken);
        var returns = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var supplierIds = returns.Select(r => r.SupplierId).Distinct().ToList();
        var suppliers = await db.Suppliers
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var result = returns.Select(r => new ReturnListItem(
            r.Id, r.ReturnNumber,
            suppliers.GetValueOrDefault(r.SupplierId, "?"),
            r.Status, r.ReturnDate, r.TotalAmount, r.Reason, r.CreatedAt))
            .ToList();

        return Result.Success(new GetReturnsResult(result, total));
    }
}
