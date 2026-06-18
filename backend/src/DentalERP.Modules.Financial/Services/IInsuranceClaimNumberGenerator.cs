namespace DentalERP.Modules.Financial.Services;

public interface IInsuranceClaimNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
