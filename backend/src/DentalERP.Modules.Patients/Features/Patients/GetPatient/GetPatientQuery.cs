using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.GetPatient;

public sealed record GetPatientQuery(Guid Id) : IRequest<Result<PatientDetail>>;

public sealed record PatientDetail(
    Guid Id,
    string FileNumber,
    string FullName,
    string Phone,
    string? Phone2,
    string? Email,
    string? Gender,
    DateOnly? DateOfBirth,
    int? Age,
    string? Address,
    string? NationalId,
    string? BloodType,
    string? Allergies,
    string? ChronicDiseases,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt
);
