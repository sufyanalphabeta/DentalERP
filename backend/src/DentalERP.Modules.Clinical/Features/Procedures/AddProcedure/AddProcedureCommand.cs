using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Procedures.AddProcedure;

public sealed record AddProcedureCommand(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    string ProcedureName,
    short? ToothId = null,
    string? Surface = null,
    string? ProcedureCode = null,
    Guid? TreatmentPlanItemId = null,
    Guid? ServiceId = null,
    string? Notes = null,
    int? DurationMinutes = null,
    bool UpdateChartEntry = false,
    string? ChartCondition = null) : IRequest<Result<Guid>>;
