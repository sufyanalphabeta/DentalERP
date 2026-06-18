using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.TreatmentPlans.UpdateTreatmentPlanStatus;

public sealed class UpdateTreatmentPlanStatusCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<UpdateTreatmentPlanStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateTreatmentPlanStatusCommand request, CancellationToken ct)
    {
        var plan = await db.TreatmentPlans.FirstOrDefaultAsync(p => p.Id == request.TreatmentPlanId, ct);
        if (plan is null)
            return Result.Failure(new Error("TreatmentPlan.NotFound", "Treatment plan not found."));

        try
        {
            switch (request.NewStatus)
            {
                case "Active":    plan.Activate(); break;
                case "Completed": plan.Complete(); break;
                case "Cancelled": plan.Cancel(); break;
                case "OnHold":    plan.PutOnHold(); break;
                default:
                    return Result.Failure(new Error("TreatmentPlan.InvalidStatus",
                        $"Cannot transition to '{request.NewStatus}'."));
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("TreatmentPlan.InvalidTransition", ex.Message));
        }

        await db.SaveChangesAsync(ct);

        if (request.NewStatus == "Active")
            await timeline.RecordAsync(plan.PatientId, PatientTimelineEvent.EventTypes.TreatmentPlanActivated,
                $"تفعيل خطة العلاج: {plan.Title}", PatientTimelineEvent.Categories.Clinical,
                actorId: request.ActorId, linkedEntityType: "TreatmentPlan", linkedEntityId: plan.Id, ct: ct);
        else if (request.NewStatus == "Completed")
            await timeline.RecordAsync(plan.PatientId, PatientTimelineEvent.EventTypes.TreatmentPlanCompleted,
                $"إتمام خطة العلاج: {plan.Title}", PatientTimelineEvent.Categories.Clinical,
                actorId: request.ActorId, linkedEntityType: "TreatmentPlan", linkedEntityId: plan.Id, ct: ct);

        return Result.Success();
    }
}
