using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.TreatmentPlans.UpdateTreatmentPlanStatus;

public sealed record UpdateTreatmentPlanStatusCommand(
    Guid TreatmentPlanId,
    string NewStatus,
    Guid ActorId) : IRequest<Result>;
