using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.CreateAsset;

internal sealed class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Result<CreateAssetResult>>
{
    private readonly AssetsDbContext _db;
    public CreateAssetCommandHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<CreateAssetResult>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        var count = await _db.Assets.IgnoreQueryFilters().CountAsync(ct);
        var assetTag = $"AST-{(count + 1):D6}";

        var asset = Asset.Create(assetTag, request.Name, request.CategoryId,
            request.PurchaseDate, request.PurchaseCost, request.Location,
            request.Notes, request.CreatedById);

        _db.Assets.Add(asset);

        _db.AuditLogs.Add(new AuditLogEntry
        {
            EntityType = "Asset",
            EntityId = asset.Id,
            Action = "Created",
            PerformedById = request.CreatedById,
            Details = $"Asset {assetTag} - {request.Name} created",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(new CreateAssetResult(asset.Id, assetTag));
    }
}
