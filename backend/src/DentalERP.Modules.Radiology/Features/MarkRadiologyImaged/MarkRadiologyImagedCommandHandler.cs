using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.MarkRadiologyImaged;

public sealed class MarkRadiologyImagedCommandHandler(RadiologyDbContext db)
    : IRequestHandler<MarkRadiologyImagedCommand, Result>
{
    public async Task<Result> Handle(MarkRadiologyImagedCommand request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        var result = order.MarkImaged();
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
