using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Appointments.UpdateAppointmentStatus;

public sealed class UpdateAppointmentStatusCommandHandler(PatientsDbContext db)
    : IRequestHandler<UpdateAppointmentStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateAppointmentStatusCommand request, CancellationToken ct)
    {
        var appointment = await db.Appointments.FindAsync([request.Id], ct);
        if (appointment is null)
            return Result.Failure(new Error("Appointment.NotFound", "الموعد غير موجود."));

        if (!Enum.TryParse<AppointmentStatus>(request.Status, out var newStatus))
            return Result.Failure(new Error("Appointment.InvalidStatus", "حالة الموعد غير صالحة."));

        switch (newStatus)
        {
            case AppointmentStatus.Confirmed: appointment.Confirm(); break;
            case AppointmentStatus.InProgress: appointment.Start(); break;
            case AppointmentStatus.Completed: appointment.Complete(); break;
            case AppointmentStatus.Cancelled: appointment.Cancel(request.CancellationReason); break;
            case AppointmentStatus.NoShow: appointment.MarkNoShow(); break;
            default:
                return Result.Failure(new Error("Appointment.InvalidTransition", "تحويل الحالة غير مسموح."));
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
