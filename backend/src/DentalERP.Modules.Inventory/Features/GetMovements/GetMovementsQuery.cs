using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.GetMovements;

public sealed record GetMovementsQuery(
    Guid? ItemId,
    string? MovementType,
    string? DestinationType,
    Guid? DestinationId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 30) : IRequest<Result<GetMovementsResult>>;

public sealed record GetMovementsResult(IReadOnlyList<MovementListItem> Movements, int Total);

public sealed record MovementListItem(
    Guid Id,
    string MovementNumber,
    string ItemName,
    string ItemCode,
    string MovementType,
    string Direction,
    decimal Quantity,
    decimal? UnitCost,
    decimal? TotalCost,
    string? DestinationType,
    Guid? DestinationId,
    bool IsNegativeStock,
    string? Notes,
    DateTime CreatedAt);
