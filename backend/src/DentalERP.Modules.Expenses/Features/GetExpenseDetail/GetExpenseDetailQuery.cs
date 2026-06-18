using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.GetExpenseDetail;

public sealed record GetExpenseDetailQuery(Guid ExpenseId) : IRequest<Result<ExpenseDetailDto>>;

public sealed record ExpenseDetailDto(
    Guid Id, string ExpenseNumber, Guid? CategoryId, string? CategoryName,
    string CostCenter, DateOnly ExpenseDate, decimal Amount, string Description,
    string? RelatedModule, Guid? RelatedEntityId, Guid? VaultId,
    string? Notes, string? AttachmentKey, string? AttachmentName,
    Guid? CreatedById, DateTime CreatedAt, DateTime? UpdatedAt
);

