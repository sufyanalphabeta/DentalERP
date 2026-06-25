using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.CreateAssetCategory;

internal sealed class CreateAssetCategoryCommandHandler : IRequestHandler<CreateAssetCategoryCommand, Result<Guid>>
{
    private readonly AssetsDbContext _db;
    public CreateAssetCategoryCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateAssetCategoryCommand request, CancellationToken ct)
    {
        var exists = await _db.AssetCategories.AnyAsync(x => x.Name == request.Name, ct);
        if (exists) return Result.Failure<Guid>(Error.Conflict("AssetCategory"));

        var cat = AssetCategory.Create(request.Name, request.NameAr, request.Description, request.DepreciationRate);
        _db.AssetCategories.Add(cat);
        await _db.SaveChangesAsync(ct);
        return Result.Success(cat.Id);
    }
}
