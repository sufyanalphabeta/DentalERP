using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Chart.UpdateChart;

public sealed record UpdateChartCommand(
    Guid PatientId,
    short ToothId,
    string Condition,
    Guid RecordedById,
    string? Surface = null,
    string? Severity = null,
    string? Notes = null,
    Guid? AppointmentId = null) : IRequest<Result<Guid>>;
