using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssetDocuments;

public sealed record GetAssetDocumentsQuery(Guid AssetId) : IRequest<Result<List<AssetDocumentDto>>>;

public sealed record AssetDocumentDto(
    Guid Id, string DocumentName, string ContentType, string? Notes,
    string? PresignedUrl, DateTime CreatedAt
);
