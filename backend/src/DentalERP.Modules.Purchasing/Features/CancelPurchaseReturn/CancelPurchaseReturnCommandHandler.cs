using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Services;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CancelPurchaseReturn;

public sealed record CancelPurchaseReturnCommand(Guid ReturnId, Guid? CancelledById) : IRequest<Result>;

public sealed class CancelPurchaseReturnCommandHandler(PurchasingDbContext db, IMovementNumberGenerator movNumGen)
    : IRequestHandler<CancelPurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure(Error.NotFound("PurchaseReturn"));

        var wasConfirmed = ret.Status == "Confirmed";

        var cancelResult = ret.Cancel();
        if (cancelResult.IsFailure) return cancelResult;

        // If the return was already confirmed (stock was deducted), reverse the movements
        if (wasConfirmed)
        {
            foreach (var item in ret.Items)
            {
                // Find the warehouse from the original outbound movement
                var originalMov = await db.StockMovements
                    .AsNoTracking()
                    .Where(m => m.ReferenceId == ret.Id && m.ItemId == item.ItemId && m.Direction == "out")
                    .FirstOrDefaultAsync(cancellationToken);

                if (originalMov is null) continue;

                var warehouseId = originalMov.WarehouseId;

                // Restore the batch quantity if we have a batch reference
                if (item.BatchId.HasValue)
                {
                    var batch = await db.StockBatches.FindAsync([item.BatchId.Value], cancellationToken);
                    if (batch is not null)
                    {
                        batch.AddQuantity(item.Quantity);
                    }
                }
                else
                {
                    // Find the oldest non-depleted batch for this item to restore into
                    var anyBatch = await db.StockBatches
                        .Where(b => b.ItemId == item.ItemId && b.WarehouseId == warehouseId)
                        .OrderBy(b => b.ReceivedDate)
                        .FirstOrDefaultAsync(cancellationToken);
                    anyBatch?.AddQuantity(item.Quantity);
                }

                // Create reversal inbound movement
                var movNumber = await movNumGen.GenerateAsync(cancellationToken);
                var reversal = StockMovement.Create(
                    movNumber, item.ItemId, warehouseId,
                    "ReturnReversal", "in",
                    item.Quantity, item.BatchId, item.UnitCost,
                    referenceId: ret.Id, referenceType: "PurchaseReturnCancel",
                    createdById: request.CancelledById);

                if (reversal.IsSuccess)
                    db.StockMovements.Add(reversal.Value);
            }
        }

        db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "PurchaseReturn",
            EntityId   = ret.Id,
            Action     = "PurchaseReturn.Cancelled",
            PerformedById = request.CancelledById,
            Details    = $"Return {ret.ReturnNumber} cancelled. Was {(wasConfirmed ? "Confirmed" : "Draft")}."
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
