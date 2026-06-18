using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Chart.GetChart;

public sealed record GetChartQuery(Guid PatientId) : IRequest<Result<GetChartResponse>>;

public sealed record GetChartResponse(
    Guid PatientId,
    IReadOnlyList<ToothChartDto> Teeth);

public sealed record ToothChartDto(
    short ToothId,
    short FdiNumber,
    string NameAr,
    string NameEn,
    string Jaw,
    string Side,
    string ToothType,
    bool IsPrimary,
    IReadOnlyList<ChartEntryDto> CurrentConditions,
    ChartEntryDto? LatestCondition);

public sealed record ChartEntryDto(
    Guid Id,
    string? Surface,
    string Condition,
    string? Severity,
    string? Notes,
    DateTime RecordedAt,
    Guid? AppointmentId);
