using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.DeletePatient;

public sealed class DeletePatientCommandHandler(PatientsDbContext db)
    : IRequestHandler<DeletePatientCommand, Result>
{
    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken ct)
    {
        var patient = await db.Patients.FindAsync([request.Id], ct);
        if (patient is null)
            return Result.Failure(new Error("Patient.NotFound", "المريض غير موجود."));

        patient.Delete();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
