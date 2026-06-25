using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssets;

internal sealed class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, Result<GetAssetsResult>>
{
    private readonly AssetsDbContext _db;
    public GetAssetsQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<GetAssetsResult>> Handle(GetAssetsQuery request, CancellationToken ct)
    {
        var query = _db.Assets.AsQueryable();

        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search) || x.AssetTag.Contains(request.Search));

        var total = await query.CountAsync(ct);

        var assetList = await query.OrderBy(x => x.AssetTag)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(ct);

        var catIds = assetList.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId!.Value).Distinct().ToList();
        var cats = catIds.Count > 0
            ? await _db.AssetCategories.AsNoTracking()
                .Where(c => catIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, ct)
            : new Dictionary<Guid, string>();

        var items = assetList.Select(x => new AssetListDto(
            x.Id, x.AssetTag, x.Name,
            x.CategoryId.HasValue && cats.TryGetValue(x.CategoryId.Value, out var cn) ? cn : null,
            x.Status, x.Location, x.PurchaseCost, x.CreatedAt, x.SerialNumber))
            .ToList();

        return Result.Success(new GetAssetsResult(items, total, request.Page, request.PageSize));
    }
}
