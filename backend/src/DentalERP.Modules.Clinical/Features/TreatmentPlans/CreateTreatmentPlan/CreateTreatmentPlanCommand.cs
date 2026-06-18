using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.TreatmentPlans.CreateTreatmentPlan;

public sealed record CreateTreatmentPlanCommand(
    Guid PatientId,
    Guid DoctorId,
    string Title,
    decimal EstimatedCost,
    string Priority = "Normal",
    string? Description = null,
    string? Notes = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    List<CreateTreatmentPlanItemDto>? Items = null) : IRequest<Result<Guid>>;

public sealed record CreateTreatmentPlanItemDto(
    string ProcedureName,
    decimal UnitPrice,
    int Quantity = 1,
    decimal DiscountPercent = 0,
    short? ToothId = null,
    string? Surface = null,
    string? ProcedureCode = null,
    int SequenceNumber = 1,
    string? Notes = null);
