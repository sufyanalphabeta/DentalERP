using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetOverdueInstallments;

public sealed class GetOverdueInstallmentsQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetOverdueInstallmentsQuery, Result<List<OverdueInstallmentDto>>>
{
    public async Task<Result<List<OverdueInstallmentDto>>> Handle(GetOverdueInstallmentsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var plans = await db.InstallmentPlans
            .Include(p => p.Installments)
            .ToListAsync(cancellationToken);

        var overduePlans = plans
            .Where(p => p.Installments.Any(i => i.Status is "Pending" or "Overdue" && i.DueDate < today))
            .ToList();

        if (overduePlans.Count == 0)
            return Result.Success(new List<OverdueInstallmentDto>());

        var invoiceIds = overduePlans.Select(p => p.InvoiceId).Distinct().ToList();
        var invoiceNumbers = await db.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, cancellationToken);

        var patientIds = overduePlans.Select(p => p.PatientId).Distinct().ToList();
        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var dtos = overduePlans.Select(p =>
        {
            var overdueItems = p.Installments
                .Where(i => i.Status is "Pending" or "Overdue" && i.DueDate < today)
                .ToList();
            var paidAmount = p.Installments.Where(i => i.Status == "Paid").Sum(i => i.Amount);

            return new OverdueInstallmentDto(
                p.Id,
                invoiceNumbers.GetValueOrDefault(p.InvoiceId, "—"),
                p.PatientId,
                patientNames.GetValueOrDefault(p.PatientId, "—"),
                p.TotalAmount,
                paidAmount,
                p.TotalAmount - paidAmount,
                overdueItems.Count,
                overdueItems.Min(i => i.DueDate)
            );
        }).OrderBy(d => d.OldestDueDate).ToList();

        return Result.Success(dtos);
    }
}
