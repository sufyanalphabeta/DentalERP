using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Appointments.GetAppointments;

public sealed class GetAppointmentsQueryHandler(PatientsDbContext db)
    : IRequestHandler<GetAppointmentsQuery, Result<GetAppointmentsResponse>>
{
    public async Task<Result<GetAppointmentsResponse>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        var query = db.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.AppointmentType)
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(a => a.ScheduledAt >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(a => a.ScheduledAt <= to);
        }

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);

        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<AppointmentStatus>(request.Status, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.ScheduledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentItem(
                a.Id,
                a.PatientId,
                a.Patient!.FullName,
                a.Patient.Phone,
                a.DoctorId,
                a.ScheduledAt,
                a.DurationMinutes,
                a.Status.ToString(),
                a.AppointmentType != null ? a.AppointmentType.Name : null,
                a.AppointmentType != null ? a.AppointmentType.NameAr : null,
                a.AppointmentType != null ? a.AppointmentType.Color : null,
                a.ChiefComplaint))
            .ToListAsync(ct);

        return Result.Success(new GetAppointmentsResponse(items, totalCount));
    }
}
