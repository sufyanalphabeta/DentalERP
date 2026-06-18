using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierItemCatalog;

public sealed record GetSupplierItemCatalogQuery(Guid SupplierId) : IRequest<Result<IReadOnlyList<CatalogItemDto>>>;

public sealed record CatalogItemDto(
    Guid SupplierItemId, Guid ItemId, string ItemName, string ItemCode,
    string SupplierItemCode, string? SupplierItemName, decimal? LastUnitCost, bool IsPreferred);

public sealed class GetSupplierItemCatalogQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSupplierItemCatalogQuery, Result<IReadOnlyList<CatalogItemDto>>>
{
    public async Task<Result<IReadOnlyList<CatalogItemDto>>> Handle(
        GetSupplierItemCatalogQuery request, CancellationToken cancellationToken)
    {
        var supplierItems = await db.SupplierItems
            .AsNoTracking()
            .Where(si => si.SupplierId == request.SupplierId)
            .ToListAsync(cancellationToken);

        var itemIds = supplierItems.Select(si => si.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var result = supplierItems.Select(si => new CatalogItemDto(
            si.Id, si.ItemId,
            items.GetValueOrDefault(si.ItemId)?.Name ?? "?",
            items.GetValueOrDefault(si.ItemId)?.ItemCode ?? "?",
            si.SupplierItemCode, si.SupplierItemName, si.LastUnitCost, si.IsPreferred))
            .ToList();

        return Result.Success<IReadOnlyList<CatalogItemDto>>(result);
    }
}
