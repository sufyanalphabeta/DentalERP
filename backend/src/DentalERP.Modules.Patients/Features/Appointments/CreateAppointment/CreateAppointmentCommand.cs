using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Appointments.CreateAppointment;

[RequirePermission("Appointments.Create")]
public sealed record CreateAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    int DurationMinutes = 30,
    Guid? AppointmentTypeId = null,
    string? ChiefComplaint = null,
    string? Notes = null
) : IRequest<Result<Guid>>;
