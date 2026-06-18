using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseOrders;

public sealed record GetPurchaseOrdersQuery(
    Guid? SupplierId, string? Status, DateTime? From, DateTime? To,
    int Page = 1, int PageSize = 20) : IRequest<Result<GetPOsResult>>;

public sealed record GetPOsResult(IReadOnlyList<POListItem> Orders, int Total);

public sealed record POListItem(
    Guid Id, string PoNumber, string SupplierName, string Status,
    DateOnly OrderDate, DateOnly? ExpectedDate, decimal TotalAmount, DateTime CreatedAt);

public sealed class GetPurchaseOrdersQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchaseOrdersQuery, Result<GetPOsResult>>
{
    public async Task<Result<GetPOsResult>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.PurchaseOrders.AsNoTracking();

        if (request.SupplierId.HasValue) query = query.Where(p => p.SupplierId == request.SupplierId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(p => p.Status == request.Status);
        if (request.From.HasValue) query = query.Where(p => p.CreatedAt >= request.From.Value);
        if (request.To.HasValue) query = query.Where(p => p.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var supplierIds = orders.Select(p => p.SupplierId).Distinct().ToList();
        var suppliers = await db.Suppliers
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var result = orders.Select(p => new POListItem(
            p.Id, p.PoNumber,
            suppliers.GetValueOrDefault(p.SupplierId, "?"),
            p.Status, p.OrderDate, p.ExpectedDate, p.TotalAmount, p.CreatedAt))
            .ToList();

        return Result.Success(new GetPOsResult(result, total));
    }
}
