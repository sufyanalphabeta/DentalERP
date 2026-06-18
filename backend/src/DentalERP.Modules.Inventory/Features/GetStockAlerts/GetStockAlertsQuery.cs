using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.GetStockAlerts;

public sealed record GetStockAlertsQuery() : IRequest<Result<StockAlertsDto>>;

public sealed record StockAlertsDto(
    IReadOnlyList<LowStockAlert> LowStockAlerts,
    IReadOnlyList<ExpiryAlert> ExpiryAlerts);

public sealed record LowStockAlert(
    Guid ItemId, string ItemCode, string Name,
    decimal CurrentStock, decimal ReorderLevel, decimal ReorderQuantity);

public sealed record ExpiryAlert(
    Guid BatchId, Guid ItemId, string ItemCode, string ItemName,
    string WarehouseName, decimal Quantity, DateOnly ExpiryDate,
    int DaysLeft, string Severity);
