using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.Modules.Inventory.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.CreateManualIssue;

public sealed class CreateManualIssueCommandHandler(InventoryDbContext db, IMovementNumberGenerator numGen)
    : IRequestHandler<CreateManualIssueCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateManualIssueCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items.FindAsync([request.ItemId], cancellationToken);
        if (item is null)
            return Result.Failure<Guid>(Error.NotFound("Item"));

        var warehouse = await db.Warehouses.FindAsync([request.WarehouseId], cancellationToken);
        if (warehouse is null)
            return Result.Failure<Guid>(Error.NotFound("Warehouse"));

        // Compute available stock
        var currentStock = await db.StockBatches
            .Where(b => b.ItemId == request.ItemId && b.WarehouseId == request.WarehouseId && !b.IsDepleted)
            .SumAsync(b => b.Quantity, cancellationToken);

        var isNegativeStock = false;

        // BR-INV-09/BR-INV-04: Check negative stock permission
        if (currentStock < request.Quantity)
        {
            var allowNegative = item.AllowNegativeStock && request.AllowNegativeStockOverride;
            if (!allowNegative)
                return Result.Failure<Guid>(new Error("Inventory.InsufficientStock",
                    $"Insufficient stock. Available: {currentStock}, Requested: {request.Quantity}"));
            isNegativeStock = true;
        }

        // BR-INV-05: FIFO batch deduction
        StockBatch? selectedBatch = null;
        if (request.BatchId.HasValue)
        {
            selectedBatch = await db.StockBatches
                .FirstOrDefaultAsync(b => b.Id == request.BatchId.Value && !b.IsDepleted, cancellationToken);
            if (selectedBatch is null)
                return Result.Failure<Guid>(new Error("Inventory.BatchNotFound", "Batch not found or fully depleted."));
        }
        else
        {
            // Auto FIFO
            var batches = await db.StockBatches
                .Where(b => b.ItemId == request.ItemId && b.WarehouseId == request.WarehouseId && !b.IsDepleted)
                .OrderBy(b => b.ExpiryDate == null)
                .ThenBy(b => b.ExpiryDate)
                .ThenBy(b => b.ReceivedDate)
                .ToListAsync(cancellationToken);

            var remaining = request.Quantity;
            foreach (var batch in batches)
            {
                if (remaining <= 0) break;
                var deduct = Math.Min(batch.Quantity, remaining);
                batch.Deduct(deduct);
                remaining -= deduct;
                selectedBatch ??= batch;
            }
        }

        if (selectedBatch != null && !request.BatchId.HasValue)
        {
            // Already deducted above in FIFO loop
        }
        else if (selectedBatch != null && request.BatchId.HasValue)
        {
            selectedBatch.Deduct(request.Quantity);
        }

        var movementNumber = await numGen.GenerateAsync(cancellationToken);

        var movementResult = StockMovement.Create(
            movementNumber, request.ItemId, request.WarehouseId, "ManualIssue", "out",
            request.Quantity, selectedBatch?.Id, item.UnitCost,
            request.DestinationType, request.DestinationId,
            isNegativeStock: isNegativeStock, notes: request.Notes, createdById: request.CreatedByUserId);

        if (movementResult.IsFailure)
            return Result.Failure<Guid>(movementResult.Error);

        db.StockMovements.Add(movementResult.Value);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(movementResult.Value.Id);
    }
}
