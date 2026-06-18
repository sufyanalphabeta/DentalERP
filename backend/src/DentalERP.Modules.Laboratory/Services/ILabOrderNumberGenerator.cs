namespace DentalERP.Modules.Laboratory.Services;

public interface ILabOrderNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
