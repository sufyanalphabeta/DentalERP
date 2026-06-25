using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssetCategories;

internal sealed class GetAssetCategoriesQueryHandler : IRequestHandler<GetAssetCategoriesQuery, Result<List<AssetCategoryDto>>>
{
    private readonly AssetsDbContext _db;
    public GetAssetCategoriesQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<List<AssetCategoryDto>>> Handle(GetAssetCategoriesQuery request, CancellationToken ct)
    {
        var query = _db.AssetCategories.AsQueryable();
        if (request.ActiveOnly) query = query.Where(x => x.IsActive);
        var items = await query.OrderBy(x => x.Name)
            .Select(x => new AssetCategoryDto(x.Id, x.Name, x.NameAr, x.Description, x.DepreciationRate, x.IsActive)).ToListAsync(ct);
        return Result.Success(items);
    }
}
