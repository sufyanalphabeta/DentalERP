using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Services;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CancelPurchaseInvoice;

public sealed record CancelPurchaseInvoiceCommand(Guid InvoiceId, Guid? CancelledById) : IRequest<Result>;

public sealed class CancelPurchaseInvoiceCommandHandler(PurchasingDbContext db, IMovementNumberGenerator movNumGen)
    : IRequestHandler<CancelPurchaseInvoiceCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await db.PurchaseInvoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken);

        if (inv is null) return Result.Failure(Error.NotFound("PurchaseInvoice"));
        if (inv.Status == "Cancelled")
            return Result.Failure(new Error("PI.AlreadyCancelled", "الفاتورة ملغاة بالفعل."));

        var wasPosted = inv.Status == "Posted";

        // Reverse stock movements if the invoice was already posted
        if (wasPosted)
        {
            foreach (var line in inv.Items)
            {
                // Find the inbound stock movement created when this invoice was posted
                var originalMov = await db.StockMovements
                    .AsNoTracking()
                    .Where(m => m.ReferenceId == inv.Id && m.ItemId == line.ItemId && m.Direction == "in"
                                && m.MovementType == "PurchaseReceipt")
                    .FirstOrDefaultAsync(cancellationToken);

                if (originalMov is null) continue;

                // Deduct quantity from the batch that was created for this receipt
                var batch = await db.StockBatches
                    .Where(b => b.ItemId == line.ItemId && b.WarehouseId == originalMov.WarehouseId
                                && !b.IsDepleted)
                    .OrderByDescending(b => b.ReceivedDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (batch is not null)
                {
                    var deductAmt = Math.Min(line.Quantity, batch.Quantity);
                    batch.Deduct(deductAmt);
                }

                // Create reversal outbound movement
                var movNumber = await movNumGen.GenerateAsync(cancellationToken);
                var reversal = StockMovement.Create(
                    movNumber, line.ItemId, originalMov.WarehouseId,
                    "InvoiceReversal", "out",
                    line.Quantity, null, line.PurchasePrice,
                    referenceId: inv.Id, referenceType: "PurchaseInvoiceCancel",
                    createdById: request.CancelledById);

                if (reversal.IsSuccess)
                    db.StockMovements.Add(reversal.Value);
            }
        }

        inv.Cancel();

        db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "PurchaseInvoice",
            EntityId   = inv.Id,
            Action     = "PurchaseInvoice.Cancelled",
            PerformedById = request.CancelledById,
            Details    = $"Invoice {inv.InvoiceNumber} cancelled. Was {(wasPosted ? "Posted — stock reversed" : "Draft")}."
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
