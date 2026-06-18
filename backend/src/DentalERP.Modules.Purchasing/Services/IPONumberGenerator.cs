namespace DentalERP.Modules.Purchasing.Services;

public interface IPONumberGenerator { Task<string> GenerateAsync(CancellationToken ct = default); }
public interface IGRNumberGenerator { Task<string> GenerateAsync(CancellationToken ct = default); }
public interface IReturnNumberGenerator { Task<string> GenerateAsync(CancellationToken ct = default); }
public interface ISupplierPaymentNumberGenerator { Task<string> GenerateAsync(CancellationToken ct = default); }
