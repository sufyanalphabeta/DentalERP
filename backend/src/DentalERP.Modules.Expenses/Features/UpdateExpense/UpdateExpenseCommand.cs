using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid ExpenseId,
    Guid? CategoryId,
    string CostCenter,
    DateOnly ExpenseDate,
    decimal Amount,
    string Description,
    string? Notes
) : IRequest<Result>;

