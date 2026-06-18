using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.SendLabOrder;

public sealed class SendLabOrderCommandHandler(LaboratoryDbContext db)
    : IRequestHandler<SendLabOrderCommand, Result>
{
    public async Task<Result> Handle(SendLabOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.LabOrders.FindAsync([request.OrderId], cancellationToken);
        if (order is null)
            return Result.Failure(new Error("LabOrder.NotFound", "الطلب غير موجود"));

        var result = order.Send();
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
