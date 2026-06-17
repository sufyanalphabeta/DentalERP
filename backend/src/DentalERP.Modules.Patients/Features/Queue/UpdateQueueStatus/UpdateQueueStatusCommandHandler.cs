using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Queue.UpdateQueueStatus;

public sealed class UpdateQueueStatusCommandHandler(PatientsDbContext db)
    : IRequestHandler<UpdateQueueStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateQueueStatusCommand request, CancellationToken ct)
    {
        var entry = await db.QueueEntries.FindAsync([request.Id], ct);
        if (entry is null)
            return Result.Failure(new Error("Queue.NotFound", "سجل الانتظار غير موجود."));

        if (!Enum.TryParse<QueueStatus>(request.Status, out var newStatus))
            return Result.Failure(new Error("Queue.InvalidStatus", "حالة الانتظار غير صالحة."));

        switch (newStatus)
        {
            case QueueStatus.Called: entry.Call(); break;
            case QueueStatus.InProgress: entry.Start(); break;
            case QueueStatus.Completed: entry.Complete(); break;
            case QueueStatus.Skipped: entry.Skip(); break;
            case QueueStatus.Waiting: entry.ResetToWaiting(); break;
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
