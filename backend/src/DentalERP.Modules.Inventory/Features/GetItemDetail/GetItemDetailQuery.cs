using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.GetItemDetail;

public sealed record GetItemDetailQuery(Guid Id) : IRequest<Result<ItemDetailDto>>;

public sealed record ItemDetailDto(
    Guid Id,
    string ItemCode,
    string? Barcode,
    string Name,
    string? NameAr,
    Guid? CategoryId,
    string? CategoryName,
    Guid? UnitOfMeasureId,
    string? UomName,
    decimal UnitCost,
    decimal SalePrice,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    bool IsExpiryTracked,
    bool AllowNegativeStock,
    string? StorageConditions,
    bool IsActive,
    string? Notes,
    decimal CurrentStock,
    decimal AvailableStock,
    IReadOnlyList<BarcodeSummary> Barcodes,
    IReadOnlyList<BatchSummary> Batches);

public sealed record BarcodeSummary(Guid Id, string Barcode, string? Label, bool IsPrimary);

public sealed record BatchSummary(
    Guid Id,
    string WarehouseName,
    string? BatchNumber,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpiryDate,
    DateOnly ReceivedDate,
    bool IsDepleted,
    string? ExpiryStatus);
