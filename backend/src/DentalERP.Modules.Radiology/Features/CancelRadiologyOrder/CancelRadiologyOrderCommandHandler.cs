using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.CancelRadiologyOrder;

public sealed class CancelRadiologyOrderCommandHandler(RadiologyDbContext db)
    : IRequestHandler<CancelRadiologyOrderCommand, Result>
{
    public async Task<Result> Handle(CancelRadiologyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        var result = order.Cancel(request.Reason);
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
