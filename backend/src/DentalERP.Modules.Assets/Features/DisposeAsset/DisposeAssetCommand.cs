using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.DisposeAsset;

public sealed record DisposeAssetCommand(Guid AssetId, Guid? DisposedById) : IRequest<Result>;
