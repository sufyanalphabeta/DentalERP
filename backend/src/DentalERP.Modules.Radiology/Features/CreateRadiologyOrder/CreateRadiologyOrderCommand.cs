using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.CreateRadiologyOrder;

public sealed record CreateRadiologyOrderCommand(
    Guid RadiologyTypeId,
    decimal Price,
    Guid? PatientId,
    bool IsExternalPatient,
    string? ExternalPatientName,
    string? ExternalPatientPhone,
    Guid? DoctorId,
    Guid? TechnicianId,
    string? Notes
) : IRequest<Result<Guid>>;
