using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseInvoiceDetail;

public sealed record GetPurchaseInvoiceDetailQuery(Guid InvoiceId) : IRequest<Result<PIDetailDto>>;

public sealed record PIDetailDto(
    Guid Id, string InvoiceNumber, DateOnly InvoiceDate,
    Guid SupplierId, string SupplierName, string? SupplierPhone,
    Guid? WarehouseId, string? WarehouseName,
    string Status, decimal Subtotal, decimal Discount, decimal NetTotal,
    string? Notes, DateTime CreatedAt, DateTime? PostedAt, DateTime? CancelledAt,
    IReadOnlyList<PIItemDto> Items);

public sealed record PIItemDto(
    Guid Id, Guid ItemId, string? ItemCode, string ItemName, string? Barcode,
    string? UnitName, decimal Quantity, decimal PurchasePrice, decimal? SalePrice,
    decimal LineTotal, DateOnly? ExpiryDate, string? BatchNumber, int SortOrder);

public sealed class GetPurchaseInvoiceDetailQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchaseInvoiceDetailQuery, Result<PIDetailDto>>
{
    public async Task<Result<PIDetailDto>> Handle(
        GetPurchaseInvoiceDetailQuery request, CancellationToken cancellationToken)
    {
        var inv = await db.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId && x.DeletedAt == null, cancellationToken);

        if (inv is null) return Result.Failure<PIDetailDto>(Error.NotFound("PurchaseInvoice"));

        var supplier = await db.Suppliers.IgnoreQueryFilters()
            .Where(s => s.Id == inv.SupplierId)
            .Select(s => new { s.Name, s.Phone })
            .FirstOrDefaultAsync(cancellationToken);

        string? warehouseName = null;
        if (inv.WarehouseId.HasValue)
        {
            warehouseName = await db.Warehouses
                .Where(w => w.Id == inv.WarehouseId.Value)
                .Select(w => w.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var items = inv.Items
            .OrderBy(x => x.SortOrder)
            .Select(x => new PIItemDto(
                x.Id, x.ItemId, x.ItemCode, x.ItemName, x.Barcode,
                x.UnitName, x.Quantity, x.PurchasePrice, x.SalePrice,
                x.LineTotal, x.ExpiryDate, x.BatchNumber, x.SortOrder))
            .ToList();

        return Result.Success(new PIDetailDto(
            inv.Id, inv.InvoiceNumber, inv.InvoiceDate,
            inv.SupplierId, supplier?.Name ?? "—", supplier?.Phone,
            inv.WarehouseId, warehouseName,
            inv.Status, inv.Subtotal, inv.Discount, inv.NetTotal,
            inv.Notes, inv.CreatedAt, inv.PostedAt, inv.CancelledAt, items));
    }
}
