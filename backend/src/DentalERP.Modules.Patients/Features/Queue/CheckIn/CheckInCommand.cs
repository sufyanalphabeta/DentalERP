using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Queue.CheckIn;

[RequirePermission("Appointments.Edit")]
public sealed record CheckInCommand(
    Guid PatientId,
    Guid? AppointmentId = null,
    Guid? DoctorId = null,
    string? Notes = null
) : IRequest<Result<CheckInResponse>>;

public sealed record CheckInResponse(Guid QueueEntryId, int TokenNumber);
