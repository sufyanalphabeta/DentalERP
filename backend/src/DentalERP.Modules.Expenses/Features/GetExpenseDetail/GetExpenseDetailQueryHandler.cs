using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.GetExpenseDetail;

internal sealed class GetExpenseDetailQueryHandler : IRequestHandler<GetExpenseDetailQuery, Result<ExpenseDetailDto>>
{
    private readonly ExpensesDbContext _db;
    public GetExpenseDetailQueryHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<ExpenseDetailDto>> Handle(GetExpenseDetailQuery request, CancellationToken ct)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(x => x.Id == request.ExpenseId, ct);
        if (expense is null)
            return Result.Failure<ExpenseDetailDto>(Error.NotFound("Expense"));

        string? categoryName = null;
        if (expense.CategoryId.HasValue)
        {
            var cat = await _db.ExpenseCategories.FindAsync(new object[] { expense.CategoryId.Value }, ct);
            categoryName = cat?.Name;
        }

        return Result.Success(new ExpenseDetailDto(
            expense.Id, expense.ExpenseNumber, expense.CategoryId, categoryName,
            expense.CostCenter, expense.ExpenseDate, expense.Amount, expense.Description,
            expense.RelatedModule, expense.RelatedEntityId, expense.VaultId,
            expense.Notes, expense.AttachmentKey, expense.AttachmentName,
            expense.CreatedById, expense.CreatedAt, expense.UpdatedAt));
    }
}

