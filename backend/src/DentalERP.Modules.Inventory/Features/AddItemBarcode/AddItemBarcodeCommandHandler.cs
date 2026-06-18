using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.AddItemBarcode;

public sealed record AddItemBarcodeCommand(Guid ItemId, string Barcode, string? Label, bool IsPrimary) : IRequest<Result<Guid>>;

public sealed class AddItemBarcodeCommandValidator : AbstractValidator<AddItemBarcodeCommand>
{
    public AddItemBarcodeCommandValidator()
    {
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddItemBarcodeCommandHandler(InventoryDbContext db)
    : IRequestHandler<AddItemBarcodeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddItemBarcodeCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Barcodes)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item is null)
            return Result.Failure<Guid>(Error.NotFound("Item"));

        // Check global barcode uniqueness
        var exists = await db.Items.AnyAsync(i => i.Barcode == request.Barcode, cancellationToken)
                  || await db.ItemBarcodes.AnyAsync(b => b.Barcode == request.Barcode, cancellationToken);

        if (exists)
            return Result.Failure<Guid>(new Error("Item.BarcodeExists", "Barcode is already in use."));

        var barcode = ItemBarcode.Create(item.Id, request.Barcode, request.Label, request.IsPrimary);
        var addResult = item.AddBarcode(barcode);
        if (addResult.IsFailure)
            return Result.Failure<Guid>(addResult.Error);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(barcode.Id);
    }
}
