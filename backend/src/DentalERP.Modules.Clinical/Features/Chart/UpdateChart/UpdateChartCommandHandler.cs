using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Chart.UpdateChart;

public sealed class UpdateChartCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<UpdateChartCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateChartCommand request, CancellationToken ct)
    {
        if (!DentalChartEntry.ValidConditions.Contains(request.Condition))
            return Result.Failure<Guid>(new Error("Chart.InvalidCondition",
                $"Condition '{request.Condition}' is not valid."));

        if (request.Surface is not null && !DentalChartEntry.ValidSurfaces.Contains(request.Surface))
            return Result.Failure<Guid>(new Error("Chart.InvalidSurface",
                $"Surface '{request.Surface}' must be one of: M, D, B, L, O."));

        if (request.Severity is not null && !DentalChartEntry.ValidSeverities.Contains(request.Severity))
            return Result.Failure<Guid>(new Error("Chart.InvalidSeverity",
                $"Severity '{request.Severity}' must be Mild, Moderate, or Severe."));

        var toothExists = await db.Teeth.AnyAsync(t => t.Id == request.ToothId, ct);
        if (!toothExists)
            return Result.Failure<Guid>(new Error("Chart.ToothNotFound",
                $"Tooth {request.ToothId} does not exist."));

        var previousEntries = await db.DentalChartEntries
            .Where(e => e.PatientId == request.PatientId
                     && e.ToothId == request.ToothId
                     && e.Surface == request.Surface
                     && e.IsCurrent)
            .ToListAsync(ct);

        foreach (var prev in previousEntries)
            prev.MarkSuperseded();

        var entry = DentalChartEntry.Create(
            request.PatientId,
            request.ToothId,
            request.Condition,
            request.RecordedById,
            request.Surface,
            request.Severity,
            request.Notes,
            request.AppointmentId);

        db.DentalChartEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        await timeline.RecordAsync(
            request.PatientId,
            PatientTimelineEvent.EventTypes.ChartUpdated,
            $"تحديث حالة السن {request.ToothId}",
            PatientTimelineEvent.Categories.Clinical,
            description: $"السن: {request.ToothId} | الحالة: {request.Condition}" +
                         (request.Surface is not null ? $" | السطح: {request.Surface}" : ""),
            actorId: request.RecordedById,
            linkedEntityType: "DentalChartEntry",
            linkedEntityId: entry.Id,
            ct: ct);

        return Result.Success(entry.Id);
    }
}
