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
        var profile = await db.DoctorProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.DoctorId, cancellationToken);

        var doctorName = await db.UserNames
            .Where(u => u.Id == request.DoctorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken) ?? "طبيب";

        var query = db.CommissionRecords
            .Where(c => c.DoctorId == request.DoctorId);

        if (request.From.HasValue)
            query = query.Where(c => c.CreatedAt >= DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc));
        if (request.To.HasValue)
            query = query.Where(c => c.CreatedAt <= DateTime.SpecifyKind(request.To.Value.AddDays(1), DateTimeKind.Utc));

        var commRecords = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var invoiceIds = commRecords.Select(c => c.InvoiceId).Distinct().ToList();
        var invoiceNumbers = await db.Invoices
            .Where(i => invoiceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.InvoiceNumber })
            .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, cancellationToken);

        var commissions = commRecords
            .Select(c => new CommissionLineDto(
                c.Id,
                c.InvoiceId,
                invoiceNumbers.GetValueOrDefault(c.InvoiceId, "—"),
                c.PaymentId,
                c.CommissionMethod,
                c.BaseAmount,
                c.CommissionRate,
                c.CommissionAmount,
                c.IsPaid,
                c.PaidAt,
                c.CreatedAt))
            .ToList();

        var totalPaid   = commissions.Where(r => r.IsPaid).Sum(r => r.CommissionAmount);
        var totalUnpaid = commissions.Where(r => !r.IsPaid).Sum(r => r.CommissionAmount);

        return Result.Success(new DoctorAccountDto(
            request.DoctorId,
            doctorName,
            profile?.CommissionMethod ?? "percentage_of_service",
            profile?.DefaultCommissionValue ?? 0,
            totalUnpaid,
            totalPaid,
            commissions));
    }
}
