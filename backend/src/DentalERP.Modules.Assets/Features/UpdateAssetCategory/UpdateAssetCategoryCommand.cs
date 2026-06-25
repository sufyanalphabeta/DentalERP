using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.UpdateAssetCategory;

public sealed record UpdateAssetCategoryCommand(
    Guid Id,
    string Name,
    string? NameAr,
    string? Description,
    decimal? DepreciationRate
) : IRequest<Result>;
