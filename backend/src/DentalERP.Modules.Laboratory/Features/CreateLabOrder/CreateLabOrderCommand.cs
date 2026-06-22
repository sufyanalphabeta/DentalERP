using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CreateLabOrder;

public sealed record CreateLabOrderItemDto(string ItemName, decimal UnitCost, short Quantity = 1, string? Description = null);

public sealed record CreateLabOrderCommand(
    Guid PatientId,
    Guid? DoctorId,
    Guid? LabId,
    Guid? ClientId,
    Guid? ProcedureId,
    string? Description,
    DateOnly? ExpectedAt,
    string? Notes,
    List<CreateLabOrderItemDto> Items,
    decimal? Revenue = null,
    Guid? CreatedByUserId = null
) : IRequest<Result<Guid>>;
