using DentalERP.Modules.Laboratory.Domain.Entities;
using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.RecordLabResult;

public sealed class RecordLabResultCommandHandler(LaboratoryDbContext db)
    : IRequestHandler<RecordLabResultCommand, Result>
{
    public async Task<Result> Handle(RecordLabResultCommand request, CancellationToken cancellationToken)
    {
        var order = await db.LabOrders.FindAsync([request.OrderId], cancellationToken);
        if (order is null)
            return Result.Failure(new Error("LabOrder.NotFound", "الطلب غير موجود"));

        var labResult = LabResult.Create(
            request.OrderId,
            request.ResultNotes,
            request.StorageBucket,
            request.StorageKey,
            request.FileName,
            request.FileSize,
            request.ReceivedById);

        var result = order.RecordResult(labResult);
        if (!result.IsSuccess) return result;

        db.LabResults.Add(labResult);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
