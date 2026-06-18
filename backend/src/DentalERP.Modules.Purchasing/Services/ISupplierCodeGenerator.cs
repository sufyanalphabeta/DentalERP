namespace DentalERP.Modules.Purchasing.Services;

public interface ISupplierCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
