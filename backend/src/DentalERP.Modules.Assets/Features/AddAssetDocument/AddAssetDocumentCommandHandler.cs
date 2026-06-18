using DentalERP.Modules.Assets.Domain.Entities;
using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.AddAssetDocument;

internal sealed class AddAssetDocumentCommandHandler : IRequestHandler<AddAssetDocumentCommand, Result<Guid>>
{
    private readonly AssetsDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public AddAssetDocumentCommandHandler(AssetsDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Guid>> Handle(AddAssetDocumentCommand request, CancellationToken ct)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(x => x.Id == request.AssetId, ct);
        if (asset is null) return Result.Failure<Guid>(Error.NotFound("Asset"));

        var fileKey = $"assets/{asset.AssetTag}/{Guid.NewGuid()}/{request.DocumentName}";
        await _fileStorage.UploadAsync("asset-documents", fileKey, request.FileStream, request.ContentType, ct);

        var doc = AssetDocument.Create(
            assetId: request.AssetId,
            fileName: request.DocumentName,
            fileKey: fileKey,
            contentType: request.ContentType,
            notes: request.Notes,
            uploadedById: request.UploadedById);

        _db.AssetDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);
        return Result.Success(doc.Id);
    }
}
