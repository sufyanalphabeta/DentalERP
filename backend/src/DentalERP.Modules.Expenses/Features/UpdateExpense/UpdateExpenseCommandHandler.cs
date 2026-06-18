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

        expense.Update(request.CategoryId, request.CostCenter, request.ExpenseDate,
            request.Amount, request.Description, request.Notes);

        _db.AuditLogs.Add(new AuditLogEntry
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Updated",
            Details = $"Expense {expense.ExpenseNumber} updated",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

