using DentalERP.Modules.Expenses.Domain.Entities;
using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.CreateExpenseCategory;

internal sealed class CreateExpenseCategoryCommandHandler
    : IRequestHandler<CreateExpenseCategoryCommand, Result<Guid>>
{
    private readonly ExpensesDbContext _db;
    public CreateExpenseCategoryCommandHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateExpenseCategoryCommand request, CancellationToken ct)
    {
        var exists = await _db.ExpenseCategories.AnyAsync(x => x.Name == request.Name, ct);
        if (exists)
            return Result.Failure<Guid>(Error.Conflict("ExpenseCategory"));

        var category = ExpenseCategory.Create(request.Name, request.NameAr, request.Description);
        _db.ExpenseCategories.Add(category);
        await _db.SaveChangesAsync(ct);
        return Result.Success(category.Id);
    }
}

