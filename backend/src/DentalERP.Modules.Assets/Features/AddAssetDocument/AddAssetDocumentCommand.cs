using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.AddAssetDocument;

public sealed record AddAssetDocumentCommand(
    Guid AssetId,
    string DocumentName,
    Stream FileStream,
    string ContentType,
    string? Notes,
    Guid? UploadedById
) : IRequest<Result<Guid>>;
