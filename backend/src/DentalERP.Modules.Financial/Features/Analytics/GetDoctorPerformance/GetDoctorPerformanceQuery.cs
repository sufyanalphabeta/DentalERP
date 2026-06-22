using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Analytics.GetDoctorPerformance;

public sealed record GetDoctorPerformanceQuery(int Months = 3) : IRequest<Result<List<DoctorPerformanceDto>>>;

public sealed record DoctorPerformanceDto(
    Guid DoctorId,
    string DoctorName,
    int InvoiceCount,
    decimal TotalRevenue,
    decimal TotalCommission,
    decimal CommissionRate
);
