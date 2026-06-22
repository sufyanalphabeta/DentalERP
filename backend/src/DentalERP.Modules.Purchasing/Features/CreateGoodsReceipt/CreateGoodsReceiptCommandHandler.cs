using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Services;
using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CreateGoodsReceipt;

public sealed record GRItemDto(
    Guid ItemId, decimal Quantity, decimal UnitCost,
    Guid? PoItemId, string? BatchNumber, DateOnly? ExpiryDate);

public sealed record CreateGoodsReceiptCommand(
    Guid SupplierId, Guid WarehouseId, DateOnly ReceiptDate,
    Guid? PoId, string? SupplierInvoiceRef, string? Notes,
    Guid? ReceivedById, IReadOnlyList<GRItemDto> Items) : IRequest<Result<Guid>>;

public sealed class CreateGoodsReceiptCommandValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreateGoodsReceiptCommandHandler(
    PurchasingDbContext db, IGRNumberGenerator grNumGen, IMovementNumberGenerator movNumGen)
    : IRequestHandler<CreateGoodsReceiptCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        // Validate PO if linked
        PurchaseOrder? po = null;
        if (request.PoId.HasValue)
        {
            po = await db.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == request.PoId.Value, cancellationToken);
            if (po is null) return Result.Failure<Guid>(Error.NotFound("PurchaseOrder"));
            if (po.Status == "Cancelled" || po.Status == "Closed")
                return Result.Failure<Guid>(new Error("PurchaseOrder.InvalidStatus", "Cannot receive goods for a cancelled or closed PO."));
        }

        var grNumber = await grNumGen.GenerateAsync(cancellationToken);
        var gr = GoodsReceipt.Create(grNumber, request.SupplierId, request.WarehouseId,
            request.ReceiptDate, request.PoId, request.SupplierInvoiceRef, request.Notes, request.ReceivedById);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var item in request.Items)
        {
            var grItem = GoodsReceiptItem.Create(gr.Id, item.ItemId, item.Quantity, item.UnitCost,
                item.PoItemId, item.BatchNumber, item.ExpiryDate);
            gr.AddItem(grItem);

            // Create stock batch
            var batch = StockBatch.Create(item.ItemId, request.WarehouseId, item.Quantity,
                item.UnitCost, today, item.BatchNumber, item.ExpiryDate);
            db.StockBatches.Add(batch);

            // Create stock movement using the sequence generator
            var movNumber = await movNumGen.GenerateAsync(cancellationToken);
            var movement = StockMovement.Create(
                movNumber,
                item.ItemId, request.WarehouseId, "PurchaseReceipt", "in",
                item.Quantity, batch.Id, item.UnitCost,
                referenceId: gr.Id, referenceType: "GoodsReceipt",
                createdById: request.ReceivedById);

            if (movement.IsSuccess)
                db.StockMovements.Add(movement.Value);

            // Update PO item received quantity if linked
            if (item.PoItemId.HasValue && po != null)
            {
                var poItem = po.Items.FirstOrDefault(i => i.Id == item.PoItemId.Value);
                poItem?.AddReceived(item.Quantity);
            }

            // Update item's unit cost (last known cost)
            var itemEntity = await db.Items.IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.Id == item.ItemId, cancellationToken);
            itemEntity?.UpdateCost(item.UnitCost);
        }

        // Update PO status
        po?.UpdateReceiptStatus();

        db.GoodsReceipts.Add(gr);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(gr.Id);
    }
}
