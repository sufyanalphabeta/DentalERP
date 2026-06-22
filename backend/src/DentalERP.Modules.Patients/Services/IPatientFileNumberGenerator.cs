namespace DentalERP.Modules.Patients.Services;

public interface IPatientFileNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
