using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Queue.GetQueue;

public sealed class GetQueueQueryHandler(PatientsDbContext db)
    : IRequestHandler<GetQueueQuery, Result<GetQueueResponse>>
{
    public async Task<Result<GetQueueResponse>> Handle(GetQueueQuery request, CancellationToken ct)
    {
        var date = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.QueueEntries
            .AsNoTracking()
            .Include(q => q.Patient)
            .Where(q => q.QueueDate == date);

        if (request.DoctorId.HasValue)
            query = query.Where(q => q.DoctorId == request.DoctorId.Value);

        var entries = await query
            .OrderBy(q => q.TokenNumber)
            .Select(q => new QueueEntryItem(
                q.Id,
                q.TokenNumber,
                q.PatientId,
                q.Patient!.FullName,
                q.Patient.Phone,
                q.DoctorId,
                q.Status.ToString(),
                q.CheckInAt,
                q.CalledAt,
                q.StartedAt,
                q.CompletedAt))
            .ToListAsync(ct);

        return Result.Success(new GetQueueResponse(date, entries));
    }
}
