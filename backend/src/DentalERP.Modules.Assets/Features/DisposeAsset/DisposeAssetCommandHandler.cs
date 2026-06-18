using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.DisposeAsset;

internal sealed class DisposeAssetCommandHandler : IRequestHandler<DisposeAssetCommand, Result>
{
    private readonly AssetsDbContext _db;
    public DisposeAssetCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result> Handle(DisposeAssetCommand request, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        if (asset is null) return Result.Failure(Error.NotFound("Asset"));

        var disposeResult = asset.Dispose();
        if (!disposeResult.IsSuccess) return disposeResult;

        _db.AuditLogs.Add(new AuditLogEntry
        {
            EntityType = "Asset", EntityId = asset.Id, Action = "Disposed",
            PerformedById = request.DisposedById,
            Details = $"Asset {asset.AssetTag} disposed", CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
