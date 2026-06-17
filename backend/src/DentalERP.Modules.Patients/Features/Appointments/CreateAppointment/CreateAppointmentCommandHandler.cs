using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandHandler(PatientsDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        var patientExists = await db.Patients.AnyAsync(p => p.Id == request.PatientId, ct);
        if (!patientExists)
            return Result.Failure<Guid>(new Error("Patient.NotFound", "المريض غير موجود."));

        var conflict = await db.Appointments
            .Where(a =>
                a.DoctorId == request.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow)
            .AnyAsync(a =>
                a.ScheduledAt < request.ScheduledAt.AddMinutes(request.DurationMinutes) &&
                a.ScheduledAt.AddMinutes(a.DurationMinutes) > request.ScheduledAt, ct);

        if (conflict)
            return Result.Failure<Guid>(new Error("Appointment.Conflict", "يوجد تعارض في مواعيد الطبيب."));

        var appointment = Appointment.Create(
            request.PatientId, request.DoctorId,
            request.ScheduledAt, request.DurationMinutes,
            request.AppointmentTypeId, request.ChiefComplaint,
            request.Notes, currentUser.IsAuthenticated ? currentUser.UserId : null);

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        return Result.Success(appointment.Id);
    }
}
