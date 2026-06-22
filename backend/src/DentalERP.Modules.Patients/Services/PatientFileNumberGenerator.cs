using DentalERP.Modules.Patients.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Services;

public sealed class PatientFileNumberGenerator(PatientsDbContext db) : IPatientFileNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('patient_file_number_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"P{year}-{seq:D5}";
    }
}
