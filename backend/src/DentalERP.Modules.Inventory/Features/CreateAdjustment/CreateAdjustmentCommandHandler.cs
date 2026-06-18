using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.Modules.Inventory.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.CreateAdjustment;

public sealed record CreateAdjustmentCommand(
    Guid ItemId,
    Guid WarehouseId,
    decimal Quantity,
    string Direction,
    string Reason,
    Guid? BatchId,
    bool AllowNegative,
    Guid? CreatedByUserId) : IRequest<Result<Guid>>;

public sealed class CreateAdjustmentCommandValidator : AbstractValidator<CreateAdjustmentCommand>
{
    public CreateAdjustmentCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Direction).Must(d => d == "in" || d == "out").WithMessage("Direction must be 'in' or 'out'.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateAdjustmentCommandHandler(InventoryDbContext db, IMovementNumberGenerator numGen)
    : IRequestHandler<CreateAdjustmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items.FindAsync([request.ItemId], cancellationToken);
        if (item is null) return Result.Failure<Guid>(Error.NotFound("Item"));

        var currentStock = await db.StockBatches
            .Where(b => b.ItemId == request.ItemId && b.WarehouseId == request.WarehouseId && !b.IsDepleted)
            .SumAsync(b => b.Quantity, cancellationToken);

        var isNegative = false;
        if (request.Direction == "out" && currentStock < request.Quantity)
        {
            if (!request.AllowNegative)
                return Result.Failure<Guid>(new Error("Inventory.InsufficientStock", "Insufficient stock for adjustment."));
            isNegative = true;
        }

        // If out adjustment, deduct from FIFO batches
        if (request.Direction == "out")
        {
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
            }
        }
        else
        {
            db.StockBatches.Add(StockBatch.Create(request.ItemId, request.WarehouseId, request.Quantity,
                item.UnitCost, DateOnly.FromDateTime(DateTime.UtcNow)));
        }

        var movementNumber = await numGen.GenerateAsync(cancellationToken);
        var movementResult = StockMovement.Create(
            movementNumber, request.ItemId, request.WarehouseId, "Adjustment", request.Direction,
            request.Quantity, request.BatchId, item.UnitCost,
            isNegativeStock: isNegative, notes: request.Reason, createdById: request.CreatedByUserId);

        if (movementResult.IsFailure) return Result.Failure<Guid>(movementResult.Error);

        db.StockMovements.Add(movementResult.Value);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(movementResult.Value.Id);
    }
}
