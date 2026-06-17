using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Patients.UpdatePatient;

public sealed class UpdatePatientCommandHandler(PatientsDbContext db)
    : IRequestHandler<UpdatePatientCommand, Result>
{
    public async Task<Result> Handle(UpdatePatientCommand request, CancellationToken ct)
    {
        var patient = await db.Patients.FindAsync([request.Id], ct);
        if (patient is null)
            return Result.Failure(new Error("Patient.NotFound", "المريض غير موجود."));

        patient.Update(
            request.FullName, request.Phone, request.DateOfBirth,
            request.Gender, request.Phone2, request.Email, request.Address,
            request.NationalId, request.BloodType, request.Allergies,
            request.ChronicDiseases, request.Notes, request.InsuranceCompanyId);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
