using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Installments.GetInstallmentPlans;

public sealed class GetInstallmentPlansQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInstallmentPlansQuery, Result<List<InstallmentPlanDto>>>
{
    public async Task<Result<List<InstallmentPlanDto>>> Handle(GetInstallmentPlansQuery request, CancellationToken cancellationToken)
    {
        var query = db.InstallmentPlans
            .Include(p => p.Installments)
            .AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(p => p.PatientId == request.PatientId.Value);

        var plans = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var patientIds = plans.Select(p => p.PatientId).Distinct().ToList();
        var invoiceIds = plans.Select(p => p.InvoiceId).Distinct().ToList();

        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var invoiceNumbers = await db.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.InvoiceNumber })
            .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, cancellationToken);

        var dtos = plans.Select(p => new InstallmentPlanDto(
            p.Id,
            p.InvoiceId,
            invoiceNumbers.GetValueOrDefault(p.InvoiceId, "—"),
            patientNames.GetValueOrDefault(p.PatientId, "—"),
            p.TotalAmount,
            p.InstallmentsCount,
            p.CreatedAt,
            p.Installments.Select(i => new InstallmentPaymentDto(
                i.Id,
                i.InstallmentNum,
                i.DueDate.ToString("yyyy-MM-dd"),
                i.Amount,
                i.Status,
                i.PaidAt,
                i.PaymentMethod)).ToList())).ToList();

        return Result.Success(dtos);
    }
}
