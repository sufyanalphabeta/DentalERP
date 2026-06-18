using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.CreateAsset;

public sealed record CreateAssetCommand(
    string Name,
    Guid? CategoryId,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    string? Location,
    string? Notes,
    Guid? CreatedById
) : IRequest<Result<CreateAssetResult>>;

public sealed record CreateAssetResult(Guid Id, string AssetTag);
