using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.GetExpenseCategories;

public sealed record GetExpenseCategoriesQuery(bool ActiveOnly = false) : IRequest<Result<List<ExpenseCategoryDto>>>;

public sealed record ExpenseCategoryDto(Guid Id, string Name, string? NameAr, string? Description, bool IsActive);

