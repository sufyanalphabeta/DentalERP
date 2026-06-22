namespace DentalERP.Modules.Expenses.Services;

internal interface IExpenseNumberGenerator
{
    Task<string> GenerateAsync(int year, CancellationToken ct = default);
}
