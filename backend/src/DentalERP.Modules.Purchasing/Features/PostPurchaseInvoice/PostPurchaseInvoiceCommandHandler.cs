using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Services;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.PostPurchaseInvoice;

public sealed record PostPurchaseInvoiceCommand(Guid InvoiceId) : IRequest<Result>;

public sealed class PostPurchaseInvoiceCommandHandler(PurchasingDbContext db, IMovementNumberGenerator movNumGen)
    : IRequestHandler<PostPurchaseInvoiceCommand, Result>
{
    public async Task<Result> Handle(PostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await db.PurchaseInvoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken);

        if (inv is null) return Result.Failure(Error.NotFound("PurchaseInvoice"));
        if (inv.Status != "Draft")
            return Result.Failure(new Error("PI.AlreadyPosted", "Invoice is not in Draft status."));
        if (!inv.Items.Any())
            return Result.Failure(new Error("PI.NoItems", "Cannot post an invoice with no items."));

        var warehouseId = inv.WarehouseId;
        if (!warehouseId.HasValue)
        {
            var defaultWarehouse = await db.Warehouses
                .Where(w => w.DeletedAt == null)
                .OrderBy(w => w.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (defaultWarehouse is null)
                return Result.Failure(new Error("PI.NoWarehouse", "No warehouse configured."));
            warehouseId = defaultWarehouse.Id;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var line in inv.Items)
        {
            var batch = StockBatch.Create(line.ItemId, warehouseId.Value, line.Quantity,
                line.PurchasePrice, today, line.BatchNumber, line.ExpiryDate);
            db.StockBatches.Add(batch);

            var movNumber = await movNumGen.GenerateAsync(cancellationToken);
            var movResult = StockMovement.Create(
                movNumber,
                line.ItemId,
                warehouseId.Value,
                "PurchaseReceipt",
                "in",
                line.Quantity,
                batchId: batch.Id,
                unitCost: line.PurchasePrice,
                referenceId: inv.Id,
                referenceType: "PurchaseInvoice");

            if (movResult.IsSuccess)
                db.StockMovements.Add(movResult.Value);

            var item = await db.Items.IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Id == line.ItemId, cancellationToken);
            item?.UpdateCost(line.PurchasePrice);
        }

        inv.Post();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
