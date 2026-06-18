using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(Guid PoId) : IRequest<Result>;

public sealed class CancelPurchaseOrderCommandHandler(PurchasingDbContext db)
    : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders.FindAsync([request.PoId], cancellationToken);
        if (po is null) return Result.Failure(Error.NotFound("PurchaseOrder"));
        var result = po.Cancel();
        if (result.IsFailure) return result;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
