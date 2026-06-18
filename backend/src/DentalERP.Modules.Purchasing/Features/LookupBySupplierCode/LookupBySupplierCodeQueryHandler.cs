using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.LookupBySupplierCode;

public sealed record LookupBySupplierCodeQuery(Guid SupplierId, string SupplierItemCode)
    : IRequest<Result<SupplierCodeLookupDto>>;

public sealed record SupplierCodeLookupDto(
    Guid ItemId, string ItemName, string ItemCode,
    string SupplierItemCode, string? SupplierItemName, decimal? LastUnitCost);

public sealed class LookupBySupplierCodeQueryHandler(PurchasingDbContext db)
    : IRequestHandler<LookupBySupplierCodeQuery, Result<SupplierCodeLookupDto>>
{
    public async Task<Result<SupplierCodeLookupDto>> Handle(
        LookupBySupplierCodeQuery request, CancellationToken cancellationToken)
    {
        var si = await db.SupplierItems
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.SupplierId == request.SupplierId &&
                     x.SupplierItemCode == request.SupplierItemCode,
                cancellationToken);

        if (si is null) return Result.Failure<SupplierCodeLookupDto>(Error.NotFound("SupplierItem"));

        var item = await db.Items.IgnoreQueryFilters()
            .Where(i => i.Id == si.ItemId)
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null) return Result.Failure<SupplierCodeLookupDto>(Error.NotFound("Item"));

        return Result.Success(new SupplierCodeLookupDto(
            item.Id, item.Name, item.ItemCode,
            si.SupplierItemCode, si.SupplierItemName, si.LastUnitCost));
    }
}
