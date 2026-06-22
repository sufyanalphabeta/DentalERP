using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.UpdateAsset;

internal sealed class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, Result>
{
    private readonly AssetsDbContext _db;
    public UpdateAssetCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateAssetCommand request, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        if (asset is null) return Result.Failure(Error.NotFound("Asset"));

        asset.Update(request.Name, request.CategoryId, request.PurchaseDate,
            request.PurchaseCost, request.Location, request.Notes);

        _db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "Asset", EntityId = asset.Id, Action = "Updated",
            Details = $"Asset {asset.AssetTag} updated", CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
