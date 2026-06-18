using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.GetLabOrders;

public sealed record GetLabOrdersQuery(
    Guid? PatientId = null,
    Guid? DoctorId = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<GetLabOrdersResponse>>;

public sealed record LabOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid PatientId,
    Guid DoctorId,
    string? LabName,
    string Status,
    decimal TotalCost,
    bool IsExternal,
    DateTime CreatedAt
);

public sealed record GetLabOrdersResponse(
    List<LabOrderSummaryDto> Items,
    int Total,
    int Page,
    int PageSize
);
