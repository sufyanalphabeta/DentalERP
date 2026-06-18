namespace DentalERP.Modules.Inventory.Services;

public interface IMovementNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
