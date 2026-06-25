using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssetCategories;

public sealed record GetAssetCategoriesQuery(bool ActiveOnly = false) : IRequest<Result<List<AssetCategoryDto>>>;
public sealed record AssetCategoryDto(Guid Id, string Name, string? NameAr, string? Description, decimal? DepreciationRate, bool IsActive);
