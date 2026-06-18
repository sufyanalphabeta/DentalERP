using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.TreatmentPlans.CreateTreatmentPlan;

public sealed class CreateTreatmentPlanCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<CreateTreatmentPlanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTreatmentPlanCommand request, CancellationToken ct)
    {
        if (!TreatmentPlan.ValidPriorities.Contains(request.Priority))
            return Result.Failure<Guid>(new Error("TreatmentPlan.InvalidPriority",
                $"Priority '{request.Priority}' is not valid."));

        var plan = TreatmentPlan.Create(
            request.PatientId,
            request.DoctorId,
            request.Title,
            request.EstimatedCost,
            request.Priority,
            request.Description,
            request.Notes,
            request.StartDate,
            request.EndDate);

        if (request.Items is { Count: > 0 })
        {
            foreach (var itemDto in request.Items)
            {
                var item = TreatmentPlanItem.Create(
                    plan.Id,
                    itemDto.ProcedureName,
                    itemDto.UnitPrice,
                    itemDto.Quantity,
                    itemDto.DiscountPercent,
                    itemDto.ToothId,
                    itemDto.Surface,
                    itemDto.ProcedureCode,
                    itemDto.SequenceNumber,
                    itemDto.Notes);
                plan.Items.Add(item);
            }
            plan.RecalculateTotalCost();
        }

        db.TreatmentPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        await timeline.RecordAsync(
            request.PatientId,
            PatientTimelineEvent.EventTypes.TreatmentPlanCreated,
            $"خطة علاج جديدة: {request.Title}",
            PatientTimelineEvent.Categories.Clinical,
            description: $"التكلفة التقديرية: {request.EstimatedCost:N2} | الأولوية: {request.Priority}",
            actorId: request.DoctorId,
            linkedEntityType: "TreatmentPlan",
            linkedEntityId: plan.Id,
            ct: ct);

        return Result.Success(plan.Id);
    }
}
