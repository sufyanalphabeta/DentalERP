using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.CreatePatient;

[RequirePermission("Patients.Create")]
public sealed record CreatePatientCommand(
    string FullName,
    string Phone,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? Phone2 = null,
    string? Email = null,
    string? Address = null,
    string? NationalId = null,
    string? BloodType = null,
    string? Allergies = null,
    string? ChronicDiseases = null,
    string? Notes = null,
    Guid? InsuranceCompanyId = null
) : IRequest<Result<CreatePatientResponse>>;

public sealed record CreatePatientResponse(Guid Id, string FileNumber);
