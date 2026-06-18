using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Expenses.Features.GenerateExpenseReport;

public sealed record GenerateExpenseReportQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    string? CostCenter,
    Guid? CategoryId
) : IRequest<Result<byte[]>>;

