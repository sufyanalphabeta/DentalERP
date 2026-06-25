using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetMovements;

public sealed class GetMovementsQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetMovementsQuery, Result<GetMovementsResult>>
{
    public async Task<Result<GetMovementsResult>> Handle(GetMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = db.StockMovements.AsNoTracking();

        if (request.ItemId.HasValue)
            query = query.Where(m => m.ItemId == request.ItemId.Value);
        if (!string.IsNullOrWhiteSpace(request.MovementType))
            query = query.Where(m => m.MovementType == request.MovementType);
        if (!string.IsNullOrWhiteSpace(request.DestinationType))
            query = query.Where(m => m.DestinationType == request.DestinationType);
        if (request.DestinationId.HasValue)
            query = query.Where(m => m.DestinationId == request.DestinationId.Value);
        if (request.From.HasValue)
        {
            var from = DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt >= from);
        }
        if (request.To.HasValue)
        {
            var to = DateTime.SpecifyKind(request.To.Value, DateTimeKind.Utc);
            query = query.Where(m => m.CreatedAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var itemIds = movements.Select(m => m.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var warehouseIds = movements.Select(m => m.WarehouseId).Distinct().ToList();
        var warehouses = await db.Warehouses.IgnoreQueryFilters()
            .Where(w => warehouseIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Name })
            .ToDictionaryAsync(w => w.Id, cancellationToken);

        var result = movements.Select(m => new MovementListItem(
            m.Id, m.MovementNumber,
            items.TryGetValue(m.ItemId, out var it) ? it.Name : "?",
            items.TryGetValue(m.ItemId, out var ic) ? ic.ItemCode : "?",
            m.MovementType, m.Direction, m.Quantity, m.UnitCost, m.TotalCost,
            warehouses.TryGetValue(m.WarehouseId, out var wh) ? wh.Name : null,
            m.DestinationType, m.DestinationId, m.IsNegativeStock, m.Notes, m.CreatedAt))
            .ToList();

        return Result.Success(new GetMovementsResult(result, total));
    }
}
