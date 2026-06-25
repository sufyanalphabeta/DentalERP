using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.DeleteAssetCategory;

internal sealed class DeleteAssetCategoryCommandHandler : IRequestHandler<DeleteAssetCategoryCommand, Result>
{
    private readonly AssetsDbContext _db;
    public DeleteAssetCategoryCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteAssetCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.AssetCategories.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (cat is null) return Result.Failure(Error.NotFound("AssetCategory"));

        cat.Delete();
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
