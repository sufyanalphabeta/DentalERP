using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Features.CompleteLabOrder;

public sealed class CompleteLabOrderCommandHandler(LaboratoryDbContext db)
    : IRequestHandler<CompleteLabOrderCommand, Result>
{
    public async Task<Result> Handle(CompleteLabOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.LabOrders.FindAsync([request.OrderId], cancellationToken);
        if (order is null)
            return Result.Failure(new Error("LabOrder.NotFound", "الطلب غير موجود"));

        var result = order.Complete();
        if (!result.IsSuccess) return result;

        // Update procedures.lab_cost if linked cross-module
        if (order.ProcedureId.HasValue)
        {
            await db.Database.ExecuteSqlAsync(
                $"UPDATE procedures SET lab_cost = {order.TotalCost} WHERE id = {order.ProcedureId.Value}",
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
