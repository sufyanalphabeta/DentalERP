using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.CreateExpenseCategory;

public sealed record CreateExpenseCategoryCommand(
    string Name,
    string? NameAr,
    string? Description
) : IRequest<Result<Guid>>;

