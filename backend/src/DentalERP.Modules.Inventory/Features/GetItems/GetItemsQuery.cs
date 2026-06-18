using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.GetItems;

public sealed record GetItemsQuery(
    string? Search,
    Guid? CategoryId,
    string? Barcode,
    bool? LowStockOnly,
    bool? ActiveOnly,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetItemsResult>>;

public sealed record GetItemsResult(IReadOnlyList<ItemListItem> Items, int Total, int Page, int PageSize);

public sealed record ItemListItem(
    Guid Id,
    string ItemCode,
    string? Barcode,
    string Name,
    string? NameAr,
    string? CategoryName,
    string? UomName,
    decimal UnitCost,
    decimal ReorderLevel,
    decimal CurrentStock,
    bool AllowNegativeStock,
    bool IsExpiryTracked,
    bool IsActive,
    bool IsLowStock);
