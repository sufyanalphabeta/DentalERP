using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Appointments.UpdateAppointmentStatus;

[RequirePermission("Appointments.Edit")]
public sealed record UpdateAppointmentStatusCommand(
    Guid Id,
    string Status,
    string? CancellationReason = null
) : IRequest<Result>;
