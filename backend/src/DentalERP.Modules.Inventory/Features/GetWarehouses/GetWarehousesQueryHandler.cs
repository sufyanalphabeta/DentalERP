using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetWarehouses;

public sealed class GetWarehousesQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetWarehousesQuery, Result<IReadOnlyList<WarehouseDto>>>
{
    public async Task<Result<IReadOnlyList<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var result = await db.Warehouses
            .AsNoTracking()
            .OrderByDescending(w => w.IsDefault)
            .ThenBy(w => w.Name)
            .Select(w => new WarehouseDto(w.Id, w.Name, w.NameAr, w.Location, w.IsDefault, w.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<WarehouseDto>>(result);
    }
}
