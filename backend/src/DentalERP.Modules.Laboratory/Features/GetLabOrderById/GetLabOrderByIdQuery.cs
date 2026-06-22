using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.GetLabOrderById;

public sealed record GetLabOrderByIdQuery(Guid Id) : IRequest<Result<LabOrderDetailDto>>;

public sealed record LabOrderItemDto(Guid Id, string ItemName, string? Description, short Quantity, decimal UnitCost, decimal TotalCost);

public sealed record LabResultDto(Guid Id, string? ResultNotes, string? FileName, DateTime ReceivedAt);

public sealed record LabOrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid PatientId,
    Guid? DoctorId,
    Guid? LabId,
    string? LabName,
    Guid? ClientId,
    string? ClientName,
    bool IsExternal,
    string Status,
    string? Description,
    DateTime? SentAt,
    DateOnly? ExpectedAt,
    DateTime? ReceivedAt,
    decimal TotalCost,
    decimal TotalRevenue,
    string Currency,
    string? Notes,
    DateTime CreatedAt,
    List<LabOrderItemDto> Items,
    List<LabResultDto> Results
);
