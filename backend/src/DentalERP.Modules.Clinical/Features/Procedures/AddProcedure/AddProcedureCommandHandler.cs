using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Procedures.AddProcedure;

public sealed class AddProcedureCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<AddProcedureCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddProcedureCommand request, CancellationToken ct)
    {
        var procedure = Procedure.Create(
            request.AppointmentId,
            request.PatientId,
            request.DoctorId,
            request.ProcedureName,
            request.ToothId,
            request.Surface,
            request.ProcedureCode,
            request.TreatmentPlanItemId,
            request.ServiceId,
            request.Notes,
            request.DurationMinutes);

        db.Procedures.Add(procedure);

        if (request.TreatmentPlanItemId.HasValue)
        {
            var item = await db.TreatmentPlanItems
                .FirstOrDefaultAsync(i => i.Id == request.TreatmentPlanItemId.Value, ct);
            if (item is not null)
            {
                item.MarkCompleted();
                var plan = await db.TreatmentPlans
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Id == item.TreatmentPlanId, ct);
                plan?.AddActualCost(item.TotalPrice);
            }
        }

        if (request.ToothId.HasValue && request.UpdateChartEntry && request.ChartCondition is not null)
        {
            var previousEntries = await db.DentalChartEntries
                .Where(e => e.PatientId == request.PatientId
                         && e.ToothId == request.ToothId.Value
                         && e.Surface == request.Surface
                         && e.IsCurrent)
                .ToListAsync(ct);

            foreach (var prev in previousEntries)
                prev.MarkSuperseded();

            var chartEntry = DentalChartEntry.Create(
                request.PatientId,
                request.ToothId.Value,
                request.ChartCondition,
                request.DoctorId,
                request.Surface,
                appointmentId: request.AppointmentId);

            db.DentalChartEntries.Add(chartEntry);
        }

        await db.SaveChangesAsync(ct);

        await timeline.RecordAsync(
            request.PatientId,
            PatientTimelineEvent.EventTypes.ProcedurePerformed,
            $"إجراء: {request.ProcedureName}",
            PatientTimelineEvent.Categories.Clinical,
            description: request.ToothId.HasValue
                ? $"السن: {request.ToothId}" + (request.Surface is not null ? $" | السطح: {request.Surface}" : "")
                : null,
            actorId: request.DoctorId,
            linkedEntityType: "Procedure",
            linkedEntityId: procedure.Id,
            ct: ct);

        return Result.Success(procedure.Id);
    }
}
