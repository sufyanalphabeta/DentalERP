using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Features.CreatePurchaseInvoice;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.UpdatePurchaseInvoice;

public sealed record UpdatePurchaseInvoiceCommand(
    Guid InvoiceId, DateOnly InvoiceDate, Guid? WarehouseId,
    decimal Discount, string? Notes,
    IReadOnlyList<PILineDto> Items) : IRequest<Result>;

public sealed class UpdatePurchaseInvoiceCommandHandler(PurchasingDbContext db)
    : IRequestHandler<UpdatePurchaseInvoiceCommand, Result>
{
    public async Task<Result> Handle(UpdatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await db.PurchaseInvoices
            .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken);

        if (inv is null) return Result.Failure(Error.NotFound("PurchaseInvoice"));
        if (inv.Status != "Draft")
            return Result.Failure(new Error("PI.NotDraft", "Only Draft invoices can be edited."));

        // Remove all existing line items via EF (the private _items list is not loaded here)
        var oldItems = db.PurchaseInvoiceItems.Where(i => i.InvoiceId == request.InvoiceId);
        db.PurchaseInvoiceItems.RemoveRange(oldItems);

        // Build new line items and compute subtotal
        var newItems = request.Items.Select((dto, i) =>
            PurchaseInvoiceItem.Create(inv.Id, dto.ItemId, dto.ItemName, dto.Quantity,
                dto.PurchasePrice, dto.ItemCode, dto.Barcode, dto.UnitName, dto.SalePrice,
                dto.ExpiryDate, dto.BatchNumber, i)).ToList();

        db.PurchaseInvoiceItems.AddRange(newItems);

        // Update header and totals directly (private _items list is empty, use UpdateDirect)
        var subtotal = newItems.Sum(i => i.LineTotal);
        inv.UpdateDirect(request.InvoiceDate, request.WarehouseId, subtotal, request.Discount, request.Notes);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
