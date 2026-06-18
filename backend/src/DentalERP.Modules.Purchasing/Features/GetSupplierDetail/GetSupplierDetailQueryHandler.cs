using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierDetail;

public sealed record GetSupplierDetailQuery(Guid SupplierId) : IRequest<Result<SupplierDetailDto>>;

public sealed record SupplierDetailDto(
    Guid Id, string SupplierCode, string Name, string? NameAr,
    string? Category, string? ContactPerson, string? Phone, string? Email,
    string? Address, int PaymentTermsDays, decimal CreditLimit,
    bool IsActive, string? Notes, decimal Balance,
    IReadOnlyList<SupplierItemDto> Items, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record SupplierItemDto(
    Guid Id, Guid ItemId, string ItemName, string ItemCode,
    string? SupplierItemCode, decimal? LastUnitCost);

public sealed class GetSupplierDetailQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetSupplierDetailQuery, Result<SupplierDetailDto>>
{
    public async Task<Result<SupplierDetailDto>> Handle(GetSupplierDetailQuery request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);

        if (supplier is null) return Result.Failure<SupplierDetailDto>(Error.NotFound("Supplier"));

        // Compute balance
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

        // Enrich catalog items with item names
        var itemIds = supplier.Items.Select(i => i.ItemId).Distinct().ToList();
        var itemNames = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var itemDtos = supplier.Items.Select(si => new SupplierItemDto(
            si.Id, si.ItemId,
            itemNames.GetValueOrDefault(si.ItemId)?.Name ?? "?",
            itemNames.GetValueOrDefault(si.ItemId)?.ItemCode ?? "?",
            si.SupplierItemCode, si.LastUnitCost))
            .ToList();

        return Result.Success(new SupplierDetailDto(
            supplier.Id, supplier.SupplierCode, supplier.Name, supplier.NameAr,
            supplier.Category, supplier.ContactPerson, supplier.Phone, supplier.Email,
            supplier.Address, supplier.PaymentTermsDays, supplier.CreditLimit,
            supplier.IsActive, supplier.Notes, balance, itemDtos,
            supplier.CreatedAt, supplier.UpdatedAt));
    }
}
