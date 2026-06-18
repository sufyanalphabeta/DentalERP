using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Services;

public sealed class CommissionEngine(FinancialDbContext db) : ICommissionEngine
{
    public async Task<CommissionRecord?> CalculateAsync(
        Guid doctorId,
        Guid invoiceId,
        Guid paymentId,
        decimal paidAmount,
        decimal labCost = 0,
        CancellationToken ct = default)
    {
        var profile = await db.DoctorProfiles
            .FirstOrDefaultAsync(p => p.UserId == doctorId, ct);

        // Doctor without a profile gets no commission
        if (profile is null) return null;

        decimal baseAmount;
        decimal rate = profile.DefaultCommissionValue;
        decimal commissionAmount;

        switch (profile.CommissionMethod)
        {
            case "percentage_of_service":
                baseAmount = paidAmount;
                commissionAmount = Math.Round(baseAmount * rate / 100, 2);
                break;

            case "fixed_amount":
                baseAmount = paidAmount;
                commissionAmount = rate; // fixed value regardless of amount
                break;

            case "percentage_of_net_service":
                baseAmount = paidAmount - labCost;
                if (baseAmount < 0) baseAmount = 0;
                commissionAmount = Math.Round(baseAmount * rate / 100, 2);
                break;

            default:
                return null;
        }

        return CommissionRecord.Create(
            doctorId: doctorId,
            invoiceId: invoiceId,
            paymentId: paymentId,
            commissionMethod: profile.CommissionMethod,
            baseAmount: baseAmount,
            commissionRate: rate,
            commissionAmount: commissionAmount);
    }
}
