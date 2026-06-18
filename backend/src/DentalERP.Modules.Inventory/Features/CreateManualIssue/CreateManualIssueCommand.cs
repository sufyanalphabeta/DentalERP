using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.CreateManualIssue;

public sealed record CreateManualIssueCommand(
    Guid ItemId,
    Guid WarehouseId,
    decimal Quantity,
    string DestinationType,
    Guid? BatchId,
    Guid? DestinationId,
    string? Notes,
    Guid? CreatedByUserId,
    bool AllowNegativeStockOverride = false) : IRequest<Result<Guid>>;
