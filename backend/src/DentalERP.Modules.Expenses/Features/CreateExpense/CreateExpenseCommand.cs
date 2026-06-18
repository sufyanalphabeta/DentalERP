using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid? CategoryId,
    string CostCenter,
    DateOnly ExpenseDate,
    decimal Amount,
    string Description,
    string? RelatedModule,
    Guid? RelatedEntityId,
    Guid? VaultId,
    string? Notes,
    Guid? CreatedById
) : IRequest<Result<Guid>>;

