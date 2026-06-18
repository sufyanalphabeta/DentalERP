using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPODetail;

public sealed record GetPODetailQuery(Guid PoId) : IRequest<Result<PODetailDto>>;

public sealed record PODetailDto(
    Guid Id, string PoNumber, Guid SupplierId, string SupplierName,
    string Status, DateOnly OrderDate, DateOnly? ExpectedDate,
    decimal TotalAmount, decimal DiscountAmount, string? Notes,
    IReadOnlyList<POItemDetailDto> Items, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record POItemDetailDto(
    Guid Id, Guid ItemId, string ItemName, string ItemCode,
    decimal QuantityOrdered, decimal QuantityReceived, decimal UnitCost,
    decimal TotalCost, Guid? SupplierItemId, string? Notes);

public sealed class GetPODetailQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPODetailQuery, Result<PODetailDto>>
{
    public async Task<Result<PODetailDto>> Handle(GetPODetailQuery request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.PoId, cancellationToken);

        if (po is null) return Result.Failure<PODetailDto>(Error.NotFound("PurchaseOrder"));

        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == po.SupplierId, cancellationToken);

        var itemIds = po.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToListAsync(cancellationToken);
        var itemMap = items.ToDictionary(i => i.Id);

        var itemDtos = po.Items.Select(i => new POItemDetailDto(
            i.Id, i.ItemId,
            itemMap.GetValueOrDefault(i.ItemId)?.Name ?? "?",
            itemMap.GetValueOrDefault(i.ItemId)?.ItemCode ?? "?",
            i.QuantityOrdered, i.QuantityReceived, i.UnitCost,
            i.TotalCost, i.SupplierItemId, i.Notes))
            .ToList();

        return Result.Success(new PODetailDto(
            po.Id, po.PoNumber, po.SupplierId, supplier?.Name ?? "?",
            po.Status, po.OrderDate, po.ExpectedDate,
            po.TotalAmount, po.DiscountAmount, po.Notes,
            itemDtos, po.CreatedAt, po.UpdatedAt));
    }
}
