using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.UpdateAssetCategory;

internal sealed class UpdateAssetCategoryCommandHandler : IRequestHandler<UpdateAssetCategoryCommand, Result>
{
    private readonly AssetsDbContext _db;
    public UpdateAssetCategoryCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateAssetCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.AssetCategories.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (cat is null) return Result.Failure(Error.NotFound("AssetCategory"));

        var duplicate = await _db.AssetCategories
            .AnyAsync(x => x.Name == request.Name && x.Id != request.Id, ct);
        if (duplicate) return Result.Failure(Error.Conflict("AssetCategory"));

        cat.Update(request.Name, request.NameAr, request.Description, request.DepreciationRate);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
