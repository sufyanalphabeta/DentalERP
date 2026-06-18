using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.CreateAssetCategory;

public sealed record CreateAssetCategoryCommand(string Name, string? NameAr, string? Description) : IRequest<Result<Guid>>;
