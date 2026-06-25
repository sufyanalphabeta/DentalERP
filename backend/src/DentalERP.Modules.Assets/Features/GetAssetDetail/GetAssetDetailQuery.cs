using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssetDetail;

public sealed record GetAssetDetailQuery(Guid AssetId) : IRequest<Result<AssetDetailDto>>;

public sealed record AssetDetailDto(
    Guid Id, string AssetTag, string Name, Guid? CategoryId, string? CategoryName,
    DateOnly? PurchaseDate, decimal? PurchaseCost, string? Location, string Status,
    string? SerialNumber, string? Notes, Guid? CreatedById, DateTime CreatedAt, DateTime? UpdatedAt
);
