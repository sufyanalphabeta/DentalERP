namespace DentalERP.Modules.Radiology.Services;

public interface IRadiologyOrderNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
