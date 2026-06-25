using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetAssetMaintenances;

public sealed record GetAssetMaintenancesQuery(Guid AssetId) : IRequest<Result<List<AssetMaintenanceDto>>>;

public sealed record AssetMaintenanceDto(
    Guid Id, DateOnly MaintenanceDate, decimal Cost,
    string Description, string? Vendor, DateOnly? NextMaintenanceDate,
    Guid? ExpenseId, DateTime CreatedAt
);
