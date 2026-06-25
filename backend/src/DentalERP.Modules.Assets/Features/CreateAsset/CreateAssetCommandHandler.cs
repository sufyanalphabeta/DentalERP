using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.Modules.Assets.Services;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.CreateAsset;

internal sealed class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Result<CreateAssetResult>>
{
    private readonly AssetsDbContext _db;
    private readonly IAssetTagGenerator _tagGen;

    public CreateAssetCommandHandler(AssetsDbContext db, IAssetTagGenerator tagGen)
    {
        _db = db;
        _tagGen = tagGen;
    }

    public async Task<Result<CreateAssetResult>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        var assetTag = await _tagGen.GenerateAsync(ct);

        var asset = Asset.Create(assetTag, request.Name, request.CategoryId,
            request.PurchaseDate, request.PurchaseCost, request.Location,
            request.SerialNumber, request.Notes, request.CreatedById);

        _db.Assets.Add(asset);

        _db.AuditLogEntries.Add(new AuditLogEntry
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
