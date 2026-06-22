using DentalERP.Modules.Expenses.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Services;

internal sealed class ExpenseNumberGenerator(ExpensesDbContext db) : IExpenseNumberGenerator
{
    public async Task<string> GenerateAsync(int year, CancellationToken ct = default)
    {
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('expense_number_seq') AS \"Value\"")
            .FirstAsync(ct);

        return $"EXP-{year}-{seq:D6}";
    }
}
