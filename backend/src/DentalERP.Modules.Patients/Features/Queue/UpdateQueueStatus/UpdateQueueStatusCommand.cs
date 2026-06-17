using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Queue.UpdateQueueStatus;

[RequirePermission("Appointments.Edit")]
public sealed record UpdateQueueStatusCommand(Guid Id, string Status) : IRequest<Result>;
