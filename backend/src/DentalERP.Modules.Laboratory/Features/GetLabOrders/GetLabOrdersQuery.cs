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
    string PatientName,
    Guid? DoctorId,
    string? DoctorName,
    string? ExternalLabName,
    string Status,
    string? Description,
    decimal TotalCost,
    decimal TotalRevenue,
    bool IsExternal,
    DateTime RequestDate
);

public sealed record GetLabOrdersResponse(
    List<LabOrderSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
