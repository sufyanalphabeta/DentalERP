using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.LookupItemByBarcode;

public sealed class LookupItemByBarcodeQueryHandler(InventoryDbContext db)
    : IRequestHandler<LookupItemByBarcodeQuery, Result<BarcodeItemDto>>
{
    public async Task<Result<BarcodeItemDto>> Handle(LookupItemByBarcodeQuery request, CancellationToken cancellationToken)
    {
        // Search primary barcode on item first
        var item = await db.Items
            .AsNoTracking()
            .Where(i => i.Barcode == request.Barcode)
            .Select(i => new BarcodeItemDto(i.Id, i.ItemCode, i.Name, i.NameAr, i.UnitCost))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is not null)
            return Result.Success(item);

        // Search item_barcodes table
        var barcode = await db.ItemBarcodes
            .AsNoTracking()
            .Where(b => b.Barcode == request.Barcode)
            .Select(b => new { b.ItemId })
            .FirstOrDefaultAsync(cancellationToken);

        if (barcode is null)
            return Result.Failure<BarcodeItemDto>(Error.NotFound("Barcode"));

        var found = await db.Items
            .AsNoTracking()
            .Where(i => i.Id == barcode.ItemId)
            .Select(i => new BarcodeItemDto(i.Id, i.ItemCode, i.Name, i.NameAr, i.UnitCost))
            .FirstOrDefaultAsync(cancellationToken);

        return found is null
            ? Result.Failure<BarcodeItemDto>(Error.NotFound("Item"))
            : Result.Success(found);
    }
}
