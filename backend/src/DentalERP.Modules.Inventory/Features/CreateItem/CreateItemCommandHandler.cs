using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.Modules.Inventory.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.CreateItem;

public sealed class CreateItemCommandHandler(InventoryDbContext db, IItemCodeGenerator codeGen)
    : IRequestHandler<CreateItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Barcode))
        {
            var barcodeExists = await db.Items
                .IgnoreQueryFilters()
                .AnyAsync(i => i.Barcode == request.Barcode, cancellationToken)
                || await db.ItemBarcodes.AnyAsync(b => b.Barcode == request.Barcode, cancellationToken);

            if (barcodeExists)
                return Result.Failure<Guid>(new Error("Item.BarcodeExists", "Barcode is already registered to another item."));
        }

        var itemCode = await codeGen.GenerateAsync(cancellationToken);

        var item = Item.Create(
            itemCode, request.Name, request.NameAr, request.Barcode,
            request.CategoryId, request.UnitOfMeasureId,
            request.UnitCost, request.ReorderLevel, request.ReorderQuantity,
            request.IsExpiryTracked, request.AllowNegativeStock,
            request.StorageConditions, request.Notes);

        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }
}
