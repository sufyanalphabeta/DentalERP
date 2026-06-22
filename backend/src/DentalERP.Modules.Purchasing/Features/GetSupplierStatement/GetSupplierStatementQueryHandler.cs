using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierStatement;

public sealed record GetSupplierStatementQuery(
    Guid SupplierId, DateTime? From, DateTime? To) : IRequest<Result<SupplierStatementDto>>;

public sealed record SupplierStatementDto(
    Guid SupplierId, string SupplierName,
    decimal OpeningBalance, decimal TotalPurchases, decimal TotalPayments,
    decimal TotalReturns, decimal ClosingBalance,
    IReadOnlyList<StatementLineDto> Lines);

public sealed record StatementLineDto(
    DateTime Date, string Type, string Reference,
    decimal Debit, decimal Credit, decimal RunningBalance);

public sealed class GetSupplierStatementQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSupplierStatementQuery, Result<SupplierStatementDto>>
{
    public async Task<Result<SupplierStatementDto>> Handle(GetSupplierStatementQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier is null) return Result.Failure<SupplierStatementDto>(Error.NotFound("Supplier"));

        var from = request.From ?? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = request.To   ?? DateTime.UtcNow.AddDays(1);

        // Purchase invoices (debit — creates supplier liability)
        var purchaseInvoices = await db.PurchaseInvoices
            .Where(p => p.SupplierId == request.SupplierId && p.Status == "Posted"
                        && p.DeletedAt == null && p.CreatedAt >= from && p.CreatedAt <= to)
            .Select(p => new { p.CreatedAt, p.InvoiceNumber, p.NetTotal })
            .ToListAsync(cancellationToken);

        // Payments (credit — we paid supplier)
        var payments = await db.SupplierPayments
            .Where(p => p.SupplierId == request.SupplierId && p.CreatedAt >= from && p.CreatedAt <= to)
            .Select(p => new { p.CreatedAt, p.PaymentNumber, p.Amount })
            .ToListAsync(cancellationToken);

        // Returns (credit — reduces our balance)
        var returns = await db.PurchaseReturns
            .Where(r => r.SupplierId == request.SupplierId && r.Status == "Confirmed"
                        && r.CreatedAt >= from && r.CreatedAt <= to)
            .Select(r => new { r.CreatedAt, r.ReturnNumber, r.TotalAmount })
            .ToListAsync(cancellationToken);

        var lines = new List<StatementLineDto>();
        decimal running = supplier.OpeningBalance;

        var allEvents = purchaseInvoices.Select(p => (p.CreatedAt, "PurchaseInvoice", p.InvoiceNumber, p.NetTotal, 0m))
            .Concat(payments.Select(p => (p.CreatedAt, "Payment", p.PaymentNumber, 0m, p.Amount)))
            .Concat(returns.Select(r => (r.CreatedAt, "Return", r.ReturnNumber, 0m, r.TotalAmount)))
            .OrderBy(e => e.CreatedAt);

        foreach (var (date, type, reference, debit, credit) in allEvents)
        {
            running += debit - credit;
            lines.Add(new StatementLineDto(date, type, reference, debit, credit, running));
        }

        var totalPurchases = purchaseInvoices.Sum(p => p.NetTotal);
        var totalPayments  = payments.Sum(p => p.Amount);
        var totalReturns   = returns.Sum(r => r.TotalAmount);

        return Result.Success(new SupplierStatementDto(
            supplier.Id, supplier.Name,
            supplier.OpeningBalance, totalPurchases, totalPayments, totalReturns,
            supplier.OpeningBalance + totalPurchases - totalPayments - totalReturns, lines));
    }
}
