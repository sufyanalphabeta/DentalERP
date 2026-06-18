using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Services;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.ConfirmPurchaseReturn;

public sealed record ConfirmPurchaseReturnCommand(Guid ReturnId, Guid? ConfirmedById) : IRequest<Result>;

public sealed class ConfirmPurchaseReturnCommandHandler(PurchasingDbContext db, IMovementNumberGenerator movNumGen)
    : IRequestHandler<ConfirmPurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(ConfirmPurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure(Error.NotFound("PurchaseReturn"));

        var confirmResult = ret.Confirm();
        if (confirmResult.IsFailure) return confirmResult;

        // Create outbound stock movements for each returned item
        foreach (var item in ret.Items)
        {
            // Resolve warehouse from batch if available
            Guid warehouseId;
            if (item.BatchId.HasValue)
            {
                var batch = await db.StockBatches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == item.BatchId.Value, cancellationToken);
                if (batch is null) continue;
                warehouseId = batch.WarehouseId;

                // Deduct from batch
                var trackedBatch = await db.StockBatches.FindAsync([item.BatchId.Value], cancellationToken);
                trackedBatch?.Deduct(item.Quantity);
            }
            else
            {
                // No batch — find any active batch for this item to get warehouse
                var anyBatch = await db.StockBatches
                    .AsNoTracking()
                    .Where(b => b.ItemId == item.ItemId && !b.IsDepleted)
                    .OrderBy(b => b.ReceivedDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (anyBatch is null) continue;
                warehouseId = anyBatch.WarehouseId;
            }

            var movNumber = await movNumGen.GenerateAsync(cancellationToken);
            var movement = StockMovement.Create(
                movNumber, item.ItemId, warehouseId,
                "SupplierReturn", "out",
                item.Quantity, item.BatchId, item.UnitCost,
                referenceId: ret.Id, referenceType: "PurchaseReturn",
                createdById: request.ConfirmedById);

            if (movement.IsSuccess)
                db.StockMovements.Add(movement.Value);
        }

        db.AuditLogs.Add(new AuditLogEntry
        {
            EntityType = "PurchaseReturn",
            EntityId   = ret.Id,
            Action     = "PurchaseReturn.Confirmed",
            PerformedById = request.ConfirmedById,
            Details    = $"Return {ret.ReturnNumber} confirmed. Total: {ret.TotalAmount}"
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
