using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierBalance;

public sealed record GetSupplierBalanceQuery(Guid SupplierId) : IRequest<Result<SupplierBalanceDto>>;

public sealed record SupplierBalanceDto(
    Guid SupplierId, string SupplierName,
    decimal OpeningBalance, decimal TotalPurchases, decimal TotalPayments, decimal TotalReturns, decimal Balance);

public sealed class GetSupplierBalanceQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSupplierBalanceQuery, Result<SupplierBalanceDto>>
{
    public async Task<Result<SupplierBalanceDto>> Handle(GetSupplierBalanceQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);

        if (supplier is null) return Result.Failure<SupplierBalanceDto>(Error.NotFound("Supplier"));

        // Only Posted Purchase Invoices create supplier liability (GoodsReceipts excluded per V1 architecture)
        var totalInvoices = await db.PurchaseInvoices
            .Where(p => p.SupplierId == request.SupplierId && p.Status == "Posted" && p.DeletedAt == null)
            .SumAsync(p => (decimal?)p.NetTotal ?? 0, cancellationToken);

        var totalPayments = await db.SupplierPayments
            .Where(p => p.SupplierId == request.SupplierId)
            .SumAsync(p => (decimal?)p.Amount ?? 0, cancellationToken);

        var totalReturns = await db.PurchaseReturns
            .Where(r => r.SupplierId == request.SupplierId && r.Status == "Confirmed")
            .SumAsync(r => (decimal?)r.TotalAmount ?? 0, cancellationToken);

        var balance = supplier.OpeningBalance + totalInvoices - totalPayments - totalReturns;

        return Result.Success(new SupplierBalanceDto(
            supplier.Id, supplier.Name, supplier.OpeningBalance,
            totalInvoices, totalPayments, totalReturns, balance));
    }
}
