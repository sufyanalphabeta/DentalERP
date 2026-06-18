using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseReturnDetail;

public sealed record GetPurchaseReturnDetailQuery(Guid ReturnId) : IRequest<Result<PurchaseReturnDetailDto>>;

public sealed record PurchaseReturnDetailDto(
    Guid Id, string ReturnNumber, Guid SupplierId, string SupplierName,
    DateOnly ReturnDate, string Reason, string Status,
    decimal TotalAmount, Guid? PoId, string? Notes,
    IReadOnlyList<ReturnItemDetailDto> Items, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record ReturnItemDetailDto(
    Guid Id, Guid ItemId, string ItemName, string ItemCode,
    Guid? BatchId, decimal Quantity, decimal UnitCost, decimal TotalCost);

public sealed class GetPurchaseReturnDetailQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchaseReturnDetailQuery, Result<PurchaseReturnDetailDto>>
{
    public async Task<Result<PurchaseReturnDetailDto>> Handle(
        GetPurchaseReturnDetailQuery request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .AsNoTracking()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure<PurchaseReturnDetailDto>(Error.NotFound("PurchaseReturn"));

        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == ret.SupplierId, cancellationToken);

        var itemIds = ret.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var itemDtos = ret.Items.Select(i => new ReturnItemDetailDto(
            i.Id, i.ItemId,
            items.GetValueOrDefault(i.ItemId)?.Name ?? "?",
            items.GetValueOrDefault(i.ItemId)?.ItemCode ?? "?",
            i.BatchId, i.Quantity, i.UnitCost, i.TotalCost))
            .ToList();

        return Result.Success(new PurchaseReturnDetailDto(
            ret.Id, ret.ReturnNumber, ret.SupplierId, supplier?.Name ?? "?",
            ret.ReturnDate, ret.Reason, ret.Status,
            ret.TotalAmount, ret.PoId, ret.Notes, itemDtos,
            ret.CreatedAt, ret.UpdatedAt));
    }
}
