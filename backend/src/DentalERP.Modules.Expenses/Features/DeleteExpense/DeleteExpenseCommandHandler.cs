using DentalERP.Modules.Expenses.Domain.Internal;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.DeleteExpense;

internal sealed class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly ExpensesDbContext _db;
    public DeleteExpenseCommandHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken ct)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == request.ExpenseId, ct);
        if (expense is null)
            return Result.Failure(Error.NotFound("Expense"));

        if (expense.VaultId.HasValue)
        {
            _db.VaultTransactions.Add(new VaultTransactionEntry
            {
                Id              = Guid.NewGuid(),
                VaultId         = expense.VaultId.Value,
                TransactionType = "general_payment",
                Amount          = expense.Amount,
                Direction       = "in",
                Notes           = $"Expense reversal: {expense.ExpenseNumber} deleted",
                CreatedByUserId = null,
                CreatedAt       = DateTime.UtcNow
            });
        }

        expense.Delete();

        _db.AuditLogEntries.Add(new DentalERP.SharedKernel.Abstractions.AuditLogEntry
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Deleted",
            Details = $"Expense {expense.ExpenseNumber} soft-deleted",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

