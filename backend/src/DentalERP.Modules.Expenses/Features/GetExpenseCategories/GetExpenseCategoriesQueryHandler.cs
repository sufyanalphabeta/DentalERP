using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.GetExpenseCategories;

internal sealed class GetExpenseCategoriesQueryHandler
    : IRequestHandler<GetExpenseCategoriesQuery, Result<List<ExpenseCategoryDto>>>
{
    private readonly ExpensesDbContext _db;
    public GetExpenseCategoriesQueryHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<List<ExpenseCategoryDto>>> Handle(GetExpenseCategoriesQuery request, CancellationToken ct)
    {
        var query = _db.ExpenseCategories.AsQueryable();
        if (request.ActiveOnly) query = query.Where(x => x.IsActive);

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new ExpenseCategoryDto(x.Id, x.Name, x.NameAr, x.Description, x.IsActive))
            .ToListAsync(ct);

        return Result.Success(items);
    }
}

