using DentalERP.Modules.Expenses.Domain.Entities;
using DentalERP.Modules.Expenses.Domain.Internal;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.CreateExpense;

internal sealed class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly ExpensesDbContext _db;
    public CreateExpenseCommandHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        if (!Expense.ValidCostCenters.Contains(request.CostCenter))
            return Result.Failure<Guid>(Error.Validation("CostCenter", $"Invalid cost center: {request.CostCenter}"));

        if (request.Amount <= 0)
            return Result.Failure<Guid>(Error.Validation("Amount", "Amount must be greater than zero."));

        var year = request.ExpenseDate.Year;
        var count = await _db.Expenses.IgnoreQueryFilters().CountAsync(ct);
        var expenseNumber = $"EXP-{year}-{(count + 1):D6}";

        var expense = Expense.Create(
            expenseNumber, request.CategoryId, request.CostCenter,
            request.ExpenseDate, request.Amount, request.Description,
            request.RelatedModule, request.RelatedEntityId,
            request.VaultId, request.Notes, request.CreatedById);

        _db.Expenses.Add(expense);

        // Atomically write vault deduction if vault provided
        if (request.VaultId.HasValue)
        {
            _db.VaultTransactions.Add(new VaultTransactionEntry
            {
                Id = Guid.NewGuid(),
                VaultId = request.VaultId.Value,
                TransactionType = "general_payment",
                Amount = request.Amount,
                Direction = "out",
                Notes = $"Expense: {request.Description}",
                CreatedByUserId = request.CreatedById,
                CreatedAt = DateTime.UtcNow
            });
        }

        _db.AuditLogs.Add(new DentalERP.SharedKernel.Abstractions.AuditLogEntry
        {
            EntityType = "Expense",
            EntityId = expense.Id,
            Action = "Created",
            PerformedById = request.CreatedById,
            Details = $"Expense {expenseNumber} created. Amount: {request.Amount} {request.CostCenter}",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(expense.Id);
    }
}

