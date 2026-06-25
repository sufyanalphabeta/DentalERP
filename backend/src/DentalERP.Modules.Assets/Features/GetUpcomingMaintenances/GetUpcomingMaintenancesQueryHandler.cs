using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetUpcomingMaintenances;

internal sealed class GetUpcomingMaintenancesQueryHandler
    : IRequestHandler<GetUpcomingMaintenancesQuery, Result<List<UpcomingMaintenanceDto>>>
{
    private readonly AssetsDbContext _db;
    public GetUpcomingMaintenancesQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<List<UpcomingMaintenanceDto>>> Handle(
        GetUpcomingMaintenancesQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(request.DaysAhead);

        // Get the most recent next_maintenance_date per asset (latest maintenance record)
        var upcoming = await _db.AssetMaintenances
            .Where(m => m.NextMaintenanceDate != null
                        && m.NextMaintenanceDate >= today
                        && m.NextMaintenanceDate <= cutoff)
            .GroupBy(m => m.AssetId)
            .Select(g => new
            {
                AssetId = g.Key,
                NextDate = g.Min(m => m.NextMaintenanceDate)
            })
            .ToListAsync(ct);

        if (upcoming.Count == 0) return Result.Success(new List<UpcomingMaintenanceDto>());

        var assetIds = upcoming.Select(u => u.AssetId).ToList();
        var assets = await _db.Assets.AsNoTracking()
            .Where(a => assetIds.Contains(a.Id))
            .Select(a => new { a.Id, a.AssetTag, a.Name })
            .ToDictionaryAsync(a => a.Id, ct);

        var result = upcoming
            .Where(u => assets.ContainsKey(u.AssetId))
            .Select(u => new UpcomingMaintenanceDto(
                u.AssetId,
                assets[u.AssetId].AssetTag,
                assets[u.AssetId].Name,
                u.NextDate!.Value,
                u.NextDate!.Value.DayNumber - today.DayNumber))
            .OrderBy(d => d.DaysUntilDue)
            .ToList();

        return Result.Success(result);
    }
}
