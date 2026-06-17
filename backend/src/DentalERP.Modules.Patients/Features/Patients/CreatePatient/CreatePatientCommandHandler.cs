using DentalERP.Modules.Patients.Domain.Entities;
using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Patients.CreatePatient;

public sealed class CreatePatientCommandHandler(PatientsDbContext db)
    : IRequestHandler<CreatePatientCommand, Result<CreatePatientResponse>>
{
    public async Task<Result<CreatePatientResponse>> Handle(CreatePatientCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            var exists = await db.Patients
                .AnyAsync(p => p.NationalId == request.NationalId, ct);
            if (exists)
                return Result.Failure<CreatePatientResponse>(
                    new Error("Patient.DuplicateNationalId", "رقم الهوية مسجّل مسبقاً."));
        }

        var fileNumber = await GenerateFileNumberAsync(ct);

        var patient = Patient.Create(
            fileNumber, request.FullName, request.Phone,
            request.DateOfBirth, request.Gender, request.Phone2,
            request.Email, request.Address, request.NationalId,
            request.BloodType, request.Allergies, request.ChronicDiseases,
            request.Notes, request.InsuranceCompanyId);

        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);

        return Result.Success(new CreatePatientResponse(patient.Id, patient.FileNumber));
    }

    private async Task<string> GenerateFileNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Patients.CountAsync(ct);
        return $"P{year}-{(count + 1):D5}";
    }
}
