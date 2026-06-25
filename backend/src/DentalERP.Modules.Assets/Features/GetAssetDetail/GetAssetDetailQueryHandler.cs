using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssetDetail;

internal sealed class GetAssetDetailQueryHandler : IRequestHandler<GetAssetDetailQuery, Result<AssetDetailDto>>
{
    private readonly AssetsDbContext _db;
    public GetAssetDetailQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<AssetDetailDto>> Handle(GetAssetDetailQuery request, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        if (asset is null) return Result.Failure<AssetDetailDto>(Error.NotFound("Asset"));

        string? catName = null;
        if (asset.CategoryId.HasValue)
            catName = (await _db.AssetCategories.FindAsync(new object[] { asset.CategoryId.Value }, ct))?.Name;

        return Result.Success(new AssetDetailDto(
            asset.Id, asset.AssetTag, asset.Name, asset.CategoryId, catName,
            asset.PurchaseDate, asset.PurchaseCost, asset.Location, asset.Status,
            asset.SerialNumber, asset.Notes, asset.CreatedById, asset.CreatedAt, asset.UpdatedAt));
    }
}
