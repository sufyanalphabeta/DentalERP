using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.CreateAssetMaintenance;

public sealed record CreateAssetMaintenanceCommand(
    Guid AssetId,
    DateOnly MaintenanceDate,
    decimal Cost,
    string Description,
    string? Vendor,
    Guid? VaultId,
    Guid? CostCategoryId,
    Guid? CreatedById
) : IRequest<Result<Guid>>;
