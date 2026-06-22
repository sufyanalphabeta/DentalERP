using DentalERP.Modules.Expenses.Domain.Entities;
using DentalERP.Modules.Expenses.Domain.Internal;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.Modules.Expenses.Services;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.CreateExpense;

internal sealed class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<Guid>>
{
    private readonly ExpensesDbContext _db;
    private readonly IExpenseNumberGenerator _numGen;

    public CreateExpenseCommandHandler(ExpensesDbContext db, IExpenseNumberGenerator numGen)
    {
        _db = db;
        _numGen = numGen;
    }

    public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        if (!request.VaultId.HasValue)
            return Result.Failure<Guid>(Error.Validation("VaultId", "يجب تحديد الخزينة لتسجيل المصروف."));

        if (!Expense.ValidCostCenters.Contains(request.CostCenter))
            return Result.Failure<Guid>(Error.Validation("CostCenter", $"Invalid cost center: {request.CostCenter}"));

        if (request.Amount <= 0)
            return Result.Failure<Guid>(Error.Validation("Amount", "Amount must be greater than zero."));

        // Validate vault existence and active status
        var vaultStatus = await _db.Database
            .SqlQuery<int?>($"SELECT CASE WHEN is_active THEN 1 ELSE 0 END AS \"Value\" FROM vaults WHERE id = {request.VaultId.Value}")
            .FirstOrDefaultAsync(ct);

        if (vaultStatus is null)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "الخزينة المحددة غير موجودة."));
        if (vaultStatus == 0)
            return Result.Failure<Guid>(new Error("Vault.Inactive", "الخزينة المحددة غير نشطة."));

        var expenseNumber = await _numGen.GenerateAsync(request.ExpenseDate.Year, ct);

        var expense = Expense.Create(
            expenseNumber, request.CategoryId, request.CostCenter,
            request.ExpenseDate, request.Amount, request.Description,
            request.RelatedModule, request.RelatedEntityId,
            request.VaultId, request.Notes, request.CreatedById);

        _db.Expenses.Add(expense);

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

        _db.AuditLogEntries.Add(new AuditLogEntry
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
