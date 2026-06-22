using DentalERP.Modules.Expenses.Domain.Internal;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.UpdateExpense;

internal sealed class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, Result>
{
    private readonly ExpensesDbContext _db;
    public UpdateExpenseCommandHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken ct)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == request.ExpenseId, ct);
        if (expense is null)
            return Result.Failure(Error.NotFound("Expense"));

        var oldAmount = expense.Amount;
        var vaultId   = expense.VaultId;

        expense.Update(request.CategoryId, request.CostCenter, request.ExpenseDate,
            request.Amount, request.Description, request.Notes);

        // If vault is linked and amount changed, create a correcting transaction
        if (vaultId.HasValue && request.Amount != oldAmount)
        {
            var diff = request.Amount - oldAmount;
            _db.VaultTransactions.Add(new VaultTransactionEntry
            {
                Id              = Guid.NewGuid(),
                VaultId         = vaultId.Value,
                TransactionType = "general_payment",
                Amount          = Math.Abs(diff),
                Direction       = diff > 0 ? "out" : "in",
                Notes           = $"Expense correction: {expense.ExpenseNumber} (old: {oldAmount:F2} → new: {request.Amount:F2})",
                CreatedByUserId = null,
                CreatedAt       = DateTime.UtcNow
            });
        }

        _db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = "Expense",
            EntityId   = expense.Id,
            Action     = "Updated",
            Details    = $"Expense {expense.ExpenseNumber} updated. Amount: {oldAmount:F2} → {request.Amount:F2}",
            CreatedAt  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
