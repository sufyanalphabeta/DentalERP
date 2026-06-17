using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Patients.GetPatient;

public sealed class GetPatientQueryHandler(PatientsDbContext db)
    : IRequestHandler<GetPatientQuery, Result<PatientDetail>>
{
    public async Task<Result<PatientDetail>> Handle(GetPatientQuery request, CancellationToken ct)
    {
        var patient = await db.Patients.AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PatientDetail(
                p.Id, p.FileNumber, p.FullName, p.Phone, p.Phone2, p.Email,
                p.Gender, p.DateOfBirth,
                p.DateOfBirth.HasValue
                    ? (int)((DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - p.DateOfBirth.Value.DayNumber) / 365.25)
                    : null,
                p.Address, p.NationalId, p.BloodType, p.Allergies, p.ChronicDiseases,
                p.Notes, p.IsActive, p.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return patient is null
            ? Result.Failure<PatientDetail>(new Error("Patient.NotFound", "Patient not found."))
            : Result.Success(patient);
    }
}
