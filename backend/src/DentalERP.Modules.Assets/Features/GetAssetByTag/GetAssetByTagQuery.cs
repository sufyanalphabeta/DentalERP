using DentalERP.Modules.Assets.Features.GetAssetDetail;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssetByTag;

public sealed record GetAssetByTagQuery(string Tag) : IRequest<Result<AssetDetailDto>>;
