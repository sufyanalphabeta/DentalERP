using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Analytics.GetMonthlyRevenue;

public sealed record GetMonthlyRevenueQuery(int Months = 6) : IRequest<Result<List<MonthlyRevenueDto>>>;

public sealed record MonthlyRevenueDto(
    int Year,
    int Month,
    string MonthLabel,
    decimal TotalRevenue,
    decimal TotalPaid,
    decimal TotalOutstanding,
    int InvoiceCount
);
