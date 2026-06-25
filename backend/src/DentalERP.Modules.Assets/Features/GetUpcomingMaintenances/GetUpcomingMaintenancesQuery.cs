using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Assets.Features.GetUpcomingMaintenances;

public sealed record GetUpcomingMaintenancesQuery(int DaysAhead = 30) : IRequest<Result<List<UpcomingMaintenanceDto>>>;

public sealed record UpcomingMaintenanceDto(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    DateOnly NextMaintenanceDate,
    int DaysUntilDue
);
