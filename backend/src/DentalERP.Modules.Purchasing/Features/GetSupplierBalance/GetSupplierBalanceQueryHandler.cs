using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierBalance;

public sealed record GetSupplierBalanceQuery(Guid SupplierId) : IRequest<Result<SupplierBalanceDto>>;

public sealed record SupplierBalanceDto(
    Guid SupplierId, string SupplierName,
    decimal TotalPurchases, decimal TotalPayments, decimal TotalReturns, decimal Balance);

public sealed class GetSupplierBalanceQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSupplierBalanceQuery, Result<SupplierBalanceDto>>
{
    public async Task<Result<SupplierBalanceDto>> Handle(GetSupplierBalanceQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);

        if (supplier is null) return Result.Failure<SupplierBalanceDto>(Error.NotFound("Supplier"));

        var totalPurchases = await db.GoodsReceipts
            .Where(g => g.SupplierId == request.SupplierId)
            .SumAsync(g => g.TotalAmount, cancellationToken);

        var totalPayments = await db.SupplierPayments
            .Where(p => p.SupplierId == request.SupplierId)
            .SumAsync(p => p.Amount, cancellationToken);

        var totalReturns = await db.PurchaseReturns
            .Where(r => r.SupplierId == request.SupplierId && r.Status == "Confirmed")
            .SumAsync(r => r.TotalAmount, cancellationToken);

        var balance = totalPurchases - totalPayments - totalReturns;

        return Result.Success(new SupplierBalanceDto(
            supplier.Id, supplier.Name, totalPurchases, totalPayments, totalReturns, balance));
    }
}
