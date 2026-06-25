using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.DeleteAssetCategory;

public sealed record DeleteAssetCategoryCommand(Guid Id) : IRequest<Result>;
