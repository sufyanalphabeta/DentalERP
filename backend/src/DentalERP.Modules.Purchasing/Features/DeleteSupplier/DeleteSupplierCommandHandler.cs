using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.DeleteSupplier;

public sealed class DeleteSupplierCommandHandler(PurchasingDbContext db)
    : IRequestHandler<DeleteSupplierCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);

        if (supplier is null) return Result.Failure<string>(Error.NotFound("Supplier"));
        if (!supplier.IsActive)
            return Result.Success("already_inactive");

        // Compute balance (GoodsReceipts excluded per V1 architecture)
        var balance = supplier.OpeningBalance
            + (await db.PurchaseInvoices.IgnoreQueryFilters()
                .Where(i => i.SupplierId == request.SupplierId && i.Status == "Posted")
                .SumAsync(i => (decimal?)i.NetTotal ?? 0, cancellationToken))
            - (await db.SupplierPayments
                .Where(p => p.SupplierId == request.SupplierId)
                .SumAsync(p => (decimal?)p.Amount ?? 0, cancellationToken))
            - (await db.PurchaseReturns.IgnoreQueryFilters()
                .Where(r => r.SupplierId == request.SupplierId && r.Status == "Confirmed")
                .SumAsync(r => (decimal?)r.TotalAmount ?? 0, cancellationToken));

        if (balance != 0)
            return Result.Failure<string>(new Error("Supplier.HasBalance",
                $"لا يمكن الإيقاف: رصيد المورد {balance:F2} د.ل — سدد الرصيد أولاً"));

        // Check all Rule 8 history conditions
        var hasInvoices = await db.PurchaseInvoices.IgnoreQueryFilters()
            .AnyAsync(i => i.SupplierId == request.SupplierId, cancellationToken);
        var hasReturns = await db.PurchaseReturns.IgnoreQueryFilters()
            .AnyAsync(r => r.SupplierId == request.SupplierId, cancellationToken);
        var hasPayments = await db.SupplierPayments
            .AnyAsync(p => p.SupplierId == request.SupplierId, cancellationToken);
        var hasItems = await db.SupplierItems
            .AnyAsync(si => si.SupplierId == request.SupplierId, cancellationToken);
        var hasAuditHistory = await db.AuditLogEntries
            .AnyAsync(a => a.EntityId == request.SupplierId, cancellationToken);

        bool hasHistory = hasInvoices || hasReturns || hasPayments || hasItems || hasAuditHistory;

        if (!hasHistory)
        {
            // Clean supplier — hard delete allowed
            db.Suppliers.Remove(supplier);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success("deleted");
        }

        // Has transaction history — deactivate only
        supplier.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success("deactivated");
    }
}
