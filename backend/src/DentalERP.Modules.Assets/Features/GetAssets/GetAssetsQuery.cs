using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssets;

public sealed record GetAssetsQuery(
    Guid? CategoryId,
    string? Status,
    string? Search,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<GetAssetsResult>>;

public sealed record GetAssetsResult(List<AssetListDto> Items, int TotalCount, int Page, int PageSize);

public sealed record AssetListDto(
    Guid Id, string AssetTag, string Name, string? CategoryName,
    string Status, string? Location, decimal? PurchaseCost, DateTime CreatedAt
);
