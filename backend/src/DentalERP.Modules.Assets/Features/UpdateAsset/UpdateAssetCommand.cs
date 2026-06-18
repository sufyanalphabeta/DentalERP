using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.UpdateAsset;

public sealed record UpdateAssetCommand(
    Guid AssetId,
    string Name,
    Guid? CategoryId,
    DateOnly? PurchaseDate,
    decimal? PurchaseCost,
    string? Location,
    string? Notes
) : IRequest<Result>;
