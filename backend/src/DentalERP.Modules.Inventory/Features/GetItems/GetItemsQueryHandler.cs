using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetItems;

public sealed class GetItemsQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetItemsQuery, Result<GetItemsResult>>
{
    public async Task<Result<GetItemsResult>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Items
            .Include(i => i.Barcodes)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(i => i.Name.Contains(request.Search) ||
                                     i.ItemCode.Contains(request.Search) ||
                                     (i.Barcode != null && i.Barcode.Contains(request.Search)));

        if (request.CategoryId.HasValue)
            query = query.Where(i => i.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Barcode))
            query = query.Where(i => i.Barcode == request.Barcode ||
                                     i.Barcodes.Any(b => b.Barcode == request.Barcode));

        if (request.ActiveOnly == true)
            query = query.Where(i => i.IsActive);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new
            {
                i.Id, i.ItemCode, i.Barcode, i.Name, i.NameAr,
                i.CategoryId, i.UnitOfMeasureId, i.UnitCost, i.SalePrice,
                i.ReorderLevel, i.AllowNegativeStock, i.IsExpiryTracked, i.IsActive
            })
            .ToListAsync(cancellationToken);

        // Compute current stock per item
        var itemIds = items.Select(i => i.Id).ToList();
        var stockByItem = await db.StockBatches
            .Where(b => itemIds.Contains(b.ItemId) && !b.IsDepleted)
            .GroupBy(b => b.ItemId)
            .Select(g => new { ItemId = g.Key, Total = g.Sum(b => b.Quantity) })
            .ToListAsync(cancellationToken);

        var stockDict = stockByItem.ToDictionary(s => s.ItemId, s => s.Total);

        var catIds = items.Where(i => i.CategoryId.HasValue).Select(i => i.CategoryId!.Value).Distinct().ToList();
        var categories = await db.ItemCategories
            .Where(c => catIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);
        var catDict = categories.ToDictionary(c => c.Id, c => c.Name);

        var uomIds = items.Where(i => i.UnitOfMeasureId.HasValue).Select(i => i.UnitOfMeasureId!.Value).Distinct().ToList();
        var uoms = await db.UnitsOfMeasure
            .Where(u => uomIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(cancellationToken);
        var uomDict = uoms.ToDictionary(u => u.Id, u => u.Name);

        var result = items.Select(i =>
        {
            var currentStock = stockDict.GetValueOrDefault(i.Id, 0);
            var isLow = currentStock <= i.ReorderLevel;
            return new ItemListItem(
                i.Id, i.ItemCode, i.Barcode, i.Name, i.NameAr,
                i.CategoryId.HasValue ? catDict.GetValueOrDefault(i.CategoryId.Value) : null,
                i.UnitOfMeasureId.HasValue ? uomDict.GetValueOrDefault(i.UnitOfMeasureId.Value) : null,
                i.UnitCost, i.SalePrice, i.ReorderLevel, currentStock,
                i.AllowNegativeStock, i.IsExpiryTracked, i.IsActive, isLow);
        }).ToList();

        if (request.LowStockOnly == true)
        {
            var lowStock = result.Where(i => i.IsLowStock).ToList();
            return Result.Success(new GetItemsResult(lowStock, lowStock.Count, request.Page, request.PageSize));
        }

        return Result.Success(new GetItemsResult(result, total, request.Page, request.PageSize));
    }
}
