using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.GetExpenses;

public sealed record GetExpensesQuery(
    string? CostCenter,
    Guid? CategoryId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? RelatedModule,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<GetExpensesResult>>;

public sealed record GetExpensesResult(List<ExpenseListDto> Items, int TotalCount, int Page, int PageSize);

public sealed record ExpenseListDto(
    Guid Id, string ExpenseNumber, string? CategoryName, string CostCenter,
    DateOnly ExpenseDate, decimal Amount, string Description,
    string? RelatedModule, Guid? RelatedEntityId, DateTime CreatedAt
);

