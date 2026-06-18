using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetStockAlerts;

public sealed class GetStockAlertsQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetStockAlertsQuery, Result<StockAlertsDto>>
{
    public async Task<Result<StockAlertsDto>> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Low stock: compute current stock per item, compare with reorder_level
        var stockByItem = await db.StockBatches
            .Where(b => !b.IsDepleted)
            .GroupBy(b => b.ItemId)
            .Select(g => new { ItemId = g.Key, Total = g.Sum(b => b.Quantity) })
            .ToListAsync(cancellationToken);

        var itemIds = stockByItem.Select(s => s.ItemId).ToList();
        var items = await db.Items
            .Where(i => itemIds.Contains(i.Id) && i.IsActive)
            .Select(i => new { i.Id, i.ItemCode, i.Name, i.ReorderLevel, i.ReorderQuantity })
            .ToListAsync(cancellationToken);

        // Also include items with zero stock and reorder_level > 0
        var zeroStockItems = await db.Items
            .Where(i => i.IsActive && i.ReorderLevel > 0 && !itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.ItemCode, i.Name, i.ReorderLevel, i.ReorderQuantity })
            .ToListAsync(cancellationToken);

        var stockDict = stockByItem.ToDictionary(s => s.ItemId, s => s.Total);

        var lowStockAlerts = items
            .Where(i => stockDict.GetValueOrDefault(i.Id, 0) <= i.ReorderLevel)
            .Select(i => new LowStockAlert(i.Id, i.ItemCode, i.Name, stockDict.GetValueOrDefault(i.Id, 0), i.ReorderLevel, i.ReorderQuantity))
            .Concat(zeroStockItems.Select(i => new LowStockAlert(i.Id, i.ItemCode, i.Name, 0, i.ReorderLevel, i.ReorderQuantity)))
            .OrderBy(a => a.CurrentStock)
            .ToList();

        // Expiry alerts: batches expiring within 90 days
        var expiryBatches = await db.StockBatches
            .Where(b => !b.IsDepleted && b.ExpiryDate != null && b.ExpiryDate <= today.AddDays(90))
            .ToListAsync(cancellationToken);

        var batchItemIds = expiryBatches.Select(b => b.ItemId).Distinct().ToList();
        var batchItems = await db.Items
            .Where(i => batchItemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.ItemCode, i.Name })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var batchWarehouseIds = expiryBatches.Select(b => b.WarehouseId).Distinct().ToList();
        var warehouses = await db.Warehouses
            .Where(w => batchWarehouseIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Name })
            .ToDictionaryAsync(w => w.Id, cancellationToken);

        var expiryAlerts = expiryBatches
            .Select(b =>
            {
                var daysLeft = b.ExpiryDate!.Value.DayNumber - today.DayNumber;
                var severity = daysLeft <= 0 ? "Expired" : daysLeft <= 30 ? "Critical" : daysLeft <= 60 ? "Warning" : "Notice";
                return new ExpiryAlert(
                    b.Id, b.ItemId,
                    batchItems.TryGetValue(b.ItemId, out var bi) ? bi.ItemCode : "?",
                    batchItems.TryGetValue(b.ItemId, out var bin) ? bin.Name : "?",
                    warehouses.TryGetValue(b.WarehouseId, out var wh) ? wh.Name : "?",
                    b.Quantity, b.ExpiryDate!.Value, daysLeft, severity);
            })
            .OrderBy(a => a.DaysLeft)
            .ToList();

        return Result.Success(new StockAlertsDto(lowStockAlerts, expiryAlerts));
    }
}
