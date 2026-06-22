namespace DentalERP.Modules.Financial.Services;

public interface IVaultTransferNumberGenerator
{
    Task<string> GenerateAsync(int year, CancellationToken ct = default);
}
