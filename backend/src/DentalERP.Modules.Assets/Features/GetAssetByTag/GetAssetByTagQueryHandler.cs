using DentalERP.Modules.Assets.Features.GetAssetDetail;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssetByTag;

internal sealed class GetAssetByTagQueryHandler : IRequestHandler<GetAssetByTagQuery, Result<AssetDetailDto>>
{
    private readonly AssetsDbContext _db;
    public GetAssetByTagQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<AssetDetailDto>> Handle(GetAssetByTagQuery request, CancellationToken ct)
    {
        var tag = request.Tag.Trim().ToUpper();
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.AssetTag == tag, ct);
        if (asset is null) return Result.Failure<AssetDetailDto>(Error.NotFound("Asset"));

        string? catName = null;
        if (asset.CategoryId.HasValue)
            catName = (await _db.AssetCategories.FindAsync(new object[] { asset.CategoryId.Value }, ct))?.Name;

        return Result.Success(new AssetDetailDto(
            asset.Id, asset.AssetTag, asset.Name, asset.CategoryId, catName,
            asset.PurchaseDate, asset.PurchaseCost, asset.Location, asset.Status,
            asset.Notes, asset.CreatedById, asset.CreatedAt, asset.UpdatedAt));
    }
}
