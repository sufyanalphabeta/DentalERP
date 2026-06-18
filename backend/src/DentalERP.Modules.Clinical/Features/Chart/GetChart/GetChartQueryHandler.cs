using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Chart.GetChart;

public sealed class GetChartQueryHandler(ClinicalDbContext db)
    : IRequestHandler<GetChartQuery, Result<GetChartResponse>>
{
    public async Task<Result<GetChartResponse>> Handle(GetChartQuery request, CancellationToken ct)
    {
        var teeth = await db.Teeth
            .OrderBy(t => t.IsPrimary)
            .ThenBy(t => t.Id)
            .ToListAsync(ct);

        var currentEntries = await db.DentalChartEntries
            .Where(e => e.PatientId == request.PatientId && e.IsCurrent)
            .Include(e => e.Tooth)
            .OrderBy(e => e.ToothId)
            .ThenBy(e => e.Surface)
            .ToListAsync(ct);

        var entriesByTooth = currentEntries
            .GroupBy(e => e.ToothId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var toothDtos = teeth.Select(t =>
        {
            var entries = entriesByTooth.TryGetValue(t.Id, out var list)
                ? list.Select(e => new ChartEntryDto(e.Id, e.Surface, e.Condition, e.Severity, e.Notes, e.RecordedAt, e.AppointmentId)).ToList()
                : [];

            return new ToothChartDto(
                t.Id,
                t.FdiNumber,
                t.NameAr,
                t.NameEn,
                t.Jaw,
                t.Side,
                t.ToothType,
                t.IsPrimary,
                entries,
                entries.MaxBy(e => e.RecordedAt));
        }).ToList();

        return Result.Success(new GetChartResponse(request.PatientId, toothDtos));
    }
}
