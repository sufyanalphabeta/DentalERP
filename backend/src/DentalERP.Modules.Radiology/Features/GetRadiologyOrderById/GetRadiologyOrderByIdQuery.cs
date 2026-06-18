using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyOrderById;

public sealed record GetRadiologyOrderByIdQuery(Guid Id) : IRequest<Result<RadiologyOrderDetailDto>>;

public sealed record RadiologyOrderDetailDto(
    Guid Id,
    string OrderNumber,
    string Status,
    Guid RadiologyTypeId,
    string RadiologyTypeName,
    decimal Price,
    bool IsExternalPatient,
    string? ExternalPatientName,
    string? ExternalPatientPhone,
    Guid? PatientId,
    Guid? DoctorId,
    Guid? TechnicianId,
    Guid? InvoiceId,
    decimal DoctorCommissionAmount,
    decimal TechCommissionAmount,
    string? Notes,
    string? CancellationReason,
    DateTime OrderDate,
    List<RadiologyImageDto> Images,
    RadiologyReportDto? Report
);

public sealed record RadiologyImageDto(Guid Id, string FileName, long FileSize, string? ContentType, DateTime UploadedAt);
public sealed record RadiologyReportDto(Guid Id, string ReportText, Guid ReportedById, DateTime ReportedAt, DateTime? UpdatedAt);
