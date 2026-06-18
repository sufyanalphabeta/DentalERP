using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Features.GetAssetDocuments;

internal sealed class GetAssetDocumentsQueryHandler : IRequestHandler<GetAssetDocumentsQuery, Result<List<AssetDocumentDto>>>
{
    private readonly AssetsDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public GetAssetDocumentsQueryHandler(AssetsDbContext db, IFileStorageService fileStorage)
    {
        _db = db; _fileStorage = fileStorage;
    }

    public async Task<Result<List<AssetDocumentDto>>> Handle(GetAssetDocumentsQuery request, CancellationToken ct)
    {
        var docs = await _db.AssetDocuments
            .Where(x => x.AssetId == request.AssetId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var result = new List<AssetDocumentDto>();
        foreach (var doc in docs)
        {
            string? url = null;
            try { url = await _fileStorage.GetPresignedUrlAsync("asset-documents", doc.FileKey, 3600, ct); }
            catch { /* storage unavailable — return null url */ }
            result.Add(new AssetDocumentDto(doc.Id, doc.FileName, doc.ContentType ?? string.Empty,
                doc.Notes, url, doc.CreatedAt));
        }
        return Result.Success(result);
    }
}
