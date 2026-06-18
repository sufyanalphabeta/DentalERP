using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid ExpenseId) : IRequest<Result>;

