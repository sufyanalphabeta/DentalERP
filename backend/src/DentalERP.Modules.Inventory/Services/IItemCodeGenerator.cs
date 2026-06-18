namespace DentalERP.Modules.Inventory.Services;

public interface IItemCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
