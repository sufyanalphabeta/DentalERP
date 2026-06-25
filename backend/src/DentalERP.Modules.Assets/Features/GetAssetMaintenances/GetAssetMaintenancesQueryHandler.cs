using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssetMaintenances;

internal sealed class GetAssetMaintenancesQueryHandler : IRequestHandler<GetAssetMaintenancesQuery, Result<List<AssetMaintenanceDto>>>
{
    private readonly AssetsDbContext _db;
    public GetAssetMaintenancesQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<List<AssetMaintenanceDto>>> Handle(GetAssetMaintenancesQuery request, CancellationToken ct)
    {
        var items = await _db.AssetMaintenances
            .Where(x => x.AssetId == request.AssetId)
            .OrderByDescending(x => x.MaintenanceDate)
            .Select(x => new AssetMaintenanceDto(
                x.Id, x.MaintenanceDate, x.Cost, x.Description, x.Vendor, x.NextMaintenanceDate, x.ExpenseId, x.CreatedAt))
            .ToListAsync(ct);

        return Result.Success(items);
    }
}
