using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyOrders;

public sealed record GetRadiologyOrdersQuery(
    Guid? PatientId,
    Guid? DoctorId,
    string? Status,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize
) : IRequest<Result<PagedRadiologyOrdersDto>>;

public sealed record PagedRadiologyOrdersDto(List<RadiologyOrderSummaryDto> Items, int TotalCount, int Page, int PageSize);

public sealed record RadiologyOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string RadiologyTypeName,
    decimal Price,
    bool IsExternalPatient,
    string? ExternalPatientName,
    Guid? PatientId,
    Guid? DoctorId,
    DateTime OrderDate
);
