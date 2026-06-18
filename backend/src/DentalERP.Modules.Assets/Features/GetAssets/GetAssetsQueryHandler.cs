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
        var cats = await _db.AssetCategories.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = await query.OrderBy(x => x.AssetTag)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new AssetListDto(
                x.Id, x.AssetTag, x.Name,
                x.CategoryId.HasValue && cats.ContainsKey(x.CategoryId.Value) ? cats[x.CategoryId.Value] : null,
                x.Status, x.Location, x.PurchaseCost, x.CreatedAt))
            .ToListAsync(ct);

        return Result.Success(new GetAssetsResult(items, total, request.Page, request.PageSize));
    }
}
