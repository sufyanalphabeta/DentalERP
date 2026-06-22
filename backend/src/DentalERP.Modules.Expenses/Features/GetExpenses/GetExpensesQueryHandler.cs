using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Expenses.Features.GetExpenses;

internal sealed class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<GetExpensesResult>>
{
    private readonly ExpensesDbContext _db;
    public GetExpensesQueryHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<GetExpensesResult>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var query = _db.Expenses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CostCenter))
            query = query.Where(x => x.CostCenter == request.CostCenter);
        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId);
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.ExpenseDate >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(x => x.ExpenseDate <= request.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.RelatedModule))
            query = query.Where(x => x.RelatedModule == request.RelatedModule);

        var total = await query.CountAsync(ct);

        var categories = await _db.ExpenseCategories.AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var rawItems = await query
            .OrderByDescending(x => x.ExpenseDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = rawItems.Select(x => new ExpenseListDto(
                x.Id, x.ExpenseNumber,
                x.CategoryId.HasValue && categories.ContainsKey(x.CategoryId.Value) ? categories[x.CategoryId.Value] : null,
                x.CostCenter, x.ExpenseDate, x.Amount, x.Description,
                x.RelatedModule, x.RelatedEntityId, x.CreatedAt,
                x.VaultId, x.CategoryId))
            .ToList();

        return Result.Success(new GetExpensesResult(items, total, request.Page, request.PageSize));
    }
}

