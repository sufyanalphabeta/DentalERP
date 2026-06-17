using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.UpdatePatient;

[RequirePermission("Patients.Edit")]
public sealed record UpdatePatientCommand(
    Guid Id,
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
) : IRequest<Result>;
