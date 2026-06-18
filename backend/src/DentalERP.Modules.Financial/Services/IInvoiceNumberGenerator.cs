namespace DentalERP.Modules.Financial.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
