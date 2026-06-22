using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetPatientBalances;

public sealed class GetPatientBalancesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetPatientBalancesQuery, Result<List<PatientBalanceDto>>>
{
    public async Task<Result<List<PatientBalanceDto>>> Handle(GetPatientBalancesQuery request, CancellationToken cancellationToken)
    {
        var balances = await db.Invoices
            .Where(i => i.Status != "Cancelled" && i.Status != "Draft" && i.TotalAmount > i.PaidAmount)
            .GroupBy(i => i.PatientId)
            .Select(g => new PatientBalanceDto(g.Key, g.Sum(i => i.TotalAmount - i.PaidAmount)))
            .ToListAsync(cancellationToken);

        return Result.Success(balances);
    }
}
