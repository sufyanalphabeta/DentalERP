using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.CompleteRadiologyOrder;

public sealed class CompleteRadiologyOrderCommandHandler(RadiologyDbContext db)
    : IRequestHandler<CompleteRadiologyOrderCommand, Result>
{
    public async Task<Result> Handle(CompleteRadiologyOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders
            .Include(o => o.Report)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        var result = order.Complete();
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
