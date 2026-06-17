using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Queue.CheckIn;

public sealed class CheckInCommandHandler(PatientsDbContext db)
    : IRequestHandler<CheckInCommand, Result<CheckInResponse>>
{
    public async Task<Result<CheckInResponse>> Handle(CheckInCommand request, CancellationToken ct)
    {
        var patientExists = await db.Patients.AnyAsync(p => p.Id == request.PatientId, ct);
        if (!patientExists)
            return Result.Failure<CheckInResponse>(new Error("Patient.NotFound", "المريض غير موجود."));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var alreadyCheckedIn = await db.QueueEntries.AnyAsync(q =>
            q.PatientId == request.PatientId &&
            q.QueueDate == today &&
            q.Status != QueueStatus.Completed &&
            q.Status != QueueStatus.Skipped, ct);

        if (alreadyCheckedIn)
            return Result.Failure<CheckInResponse>(new Error("Queue.AlreadyCheckedIn", "المريض موجود بالفعل في قائمة الانتظار اليوم."));

        var lastToken = await db.QueueEntries
            .Where(q => q.QueueDate == today)
            .MaxAsync(q => (int?)q.TokenNumber, ct) ?? 0;

        var entry = QueueEntry.Create(
            request.PatientId, lastToken + 1, today,
            request.AppointmentId, request.DoctorId, request.Notes);

        db.QueueEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        return Result.Success(new CheckInResponse(entry.Id, entry.TokenNumber));
    }
}
