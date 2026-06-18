using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetGoodsReceiptDetail;

public sealed record GetGoodsReceiptDetailQuery(Guid GrId) : IRequest<Result<GRDetailDto>>;

public sealed record GRDetailDto(
    Guid Id, string GrNumber, Guid SupplierId, string SupplierName,
    Guid WarehouseId, string WarehouseName, DateOnly ReceiptDate,
    Guid? PoId, string? SupplierInvoiceRef, decimal TotalAmount,
    string? Notes, IReadOnlyList<GRItemDetailDto> Items, DateTime CreatedAt);

public sealed record GRItemDetailDto(
    Guid Id, Guid ItemId, string ItemName, string ItemCode,
    decimal Quantity, decimal UnitCost, decimal TotalCost,
    string? BatchNumber, DateOnly? ExpiryDate, Guid? PoItemId);

public sealed class GetGoodsReceiptDetailQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetGoodsReceiptDetailQuery, Result<GRDetailDto>>
{
    public async Task<Result<GRDetailDto>> Handle(GetGoodsReceiptDetailQuery request, CancellationToken cancellationToken)
    {
        var gr = await db.GoodsReceipts
            .AsNoTracking()
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == request.GrId, cancellationToken);

        if (gr is null) return Result.Failure<GRDetailDto>(Error.NotFound("GoodsReceipt"));

        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == gr.SupplierId, cancellationToken);

        var warehouse = await db.Warehouses.AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == gr.WarehouseId, cancellationToken);

        var itemIds = gr.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToListAsync(cancellationToken);
        var itemMap = items.ToDictionary(i => i.Id);

        var itemDtos = gr.Items.Select(i => new GRItemDetailDto(
            i.Id, i.ItemId,
            itemMap.GetValueOrDefault(i.ItemId)?.Name ?? "?",
            itemMap.GetValueOrDefault(i.ItemId)?.ItemCode ?? "?",
            i.Quantity, i.UnitCost, i.TotalCost,
            i.BatchNumber, i.ExpiryDate, i.PoItemId))
            .ToList();

        return Result.Success(new GRDetailDto(
            gr.Id, gr.GrNumber, gr.SupplierId, supplier?.Name ?? "?",
            gr.WarehouseId, warehouse?.Name ?? "?",
            gr.ReceiptDate, gr.PoId, gr.SupplierInvoiceRef,
            gr.TotalAmount, gr.Notes, itemDtos, gr.CreatedAt));
    }
}
