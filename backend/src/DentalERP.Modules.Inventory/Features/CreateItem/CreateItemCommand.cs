using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.CreateItem;

public sealed record CreateItemCommand(
    string Name,
    string? NameAr,
    string? Barcode,
    Guid? CategoryId,
    Guid? UnitOfMeasureId,
    decimal UnitCost,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    bool IsExpiryTracked,
    bool AllowNegativeStock,
    string? StorageConditions,
    string? Notes,
    Guid? CreatedByUserId) : IRequest<Result<Guid>>;
