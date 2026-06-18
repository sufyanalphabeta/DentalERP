using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Commissions.GetDoctorAccount;

public sealed class GetDoctorAccountQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetDoctorAccountQuery, Result<DoctorAccountDto>>
{
    public async Task<Result<DoctorAccountDto>> Handle(GetDoctorAccountQuery request, CancellationToken cancellationToken)
    {
        var query = db.CommissionRecords
            .Where(c => c.DoctorId == request.DoctorId);

        if (request.From.HasValue)
            query = query.Where(c => c.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(c => c.CreatedAt <= request.To.Value);

        var records = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommissionLineDto(
                c.Id, c.InvoiceId, c.CommissionMethod,
                c.BaseAmount, c.CommissionRate, c.CommissionAmount,
                c.IsPaid, c.PaidAt, c.CreatedAt))
            .ToListAsync(cancellationToken);

        var due = records.Sum(r => r.CommissionAmount);
        var paid = records.Where(r => r.IsPaid).Sum(r => r.CommissionAmount);

        return Result.Success(new DoctorAccountDto(
            request.DoctorId,
            due,
            paid,
            due - paid,
            records));
    }
}
