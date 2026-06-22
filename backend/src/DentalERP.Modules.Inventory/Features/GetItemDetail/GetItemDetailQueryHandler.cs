using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetItemDetail;

public sealed class GetItemDetailQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetItemDetailQuery, Result<ItemDetailDto>>
{
    public async Task<Result<ItemDetailDto>> Handle(GetItemDetailQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Barcodes)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
            return Result.Failure<ItemDetailDto>(Error.NotFound("Item"));

        var categoryName = item.CategoryId.HasValue
            ? (await db.ItemCategories.FindAsync([item.CategoryId.Value], cancellationToken))?.Name
            : null;

        var uomName = item.UnitOfMeasureId.HasValue
            ? (await db.UnitsOfMeasure.FindAsync([item.UnitOfMeasureId.Value], cancellationToken))?.Name
            : null;

        var batches = await db.StockBatches
            .Where(b => b.ItemId == item.Id && !b.IsDepleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var warehouseIds = batches.Select(b => b.WarehouseId).Distinct().ToList();
        var warehouses = await db.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Name })
            .ToListAsync(cancellationToken);
        var whDict = warehouses.ToDictionary(w => w.Id, w => w.Name);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        string? ExpiryStatus(DateOnly? expiry)
        {
            if (!expiry.HasValue) return null;
            var daysLeft = expiry.Value.DayNumber - today.DayNumber;
            if (daysLeft < 0)  return "Expired";
            if (daysLeft <= 30) return "Critical";
            if (daysLeft <= 60) return "Warning";
            if (daysLeft <= 90) return "Notice";
            return null;
        }

        var batchDtos = batches
            .OrderBy(b => b.ExpiryDate ?? DateOnly.MaxValue)
            .ThenBy(b => b.ReceivedDate)
            .Select(b => new BatchSummary(
                b.Id,
                whDict.GetValueOrDefault(b.WarehouseId, "Unknown"),
                b.BatchNumber, b.Quantity, b.UnitCost,
                b.ExpiryDate, b.ReceivedDate, b.IsDepleted,
                ExpiryStatus(b.ExpiryDate)))
            .ToList();

        var currentStock = batches.Sum(b => b.Quantity);

        return Result.Success(new ItemDetailDto(
            item.Id, item.ItemCode, item.Barcode, item.Name, item.NameAr,
            item.CategoryId, categoryName, item.UnitOfMeasureId, uomName,
            item.UnitCost, item.SalePrice, item.ReorderLevel, item.ReorderQuantity,
            item.IsExpiryTracked, item.AllowNegativeStock,
            item.StorageConditions, item.IsActive, item.Notes,
            currentStock, currentStock,
            item.Barcodes.Select(b => new BarcodeSummary(b.Id, b.Barcode, b.Label, b.IsPrimary)).ToList(),
            batchDtos));
    }
}
