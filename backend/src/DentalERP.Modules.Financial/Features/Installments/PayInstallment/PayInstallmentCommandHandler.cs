using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Installments.PayInstallment;

public sealed class PayInstallmentCommandHandler(FinancialDbContext db)
    : IRequestHandler<PayInstallmentCommand, Result>
{
    public async Task<Result> Handle(PayInstallmentCommand request, CancellationToken cancellationToken)
    {
        var plan = await db.InstallmentPlans
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan is null)
            return Result.Failure(new Error("InstallmentPlan.NotFound", "خطة الأقساط غير موجودة"));

        var installment = plan.Installments
            .FirstOrDefault(i => i.InstallmentNum == request.InstallmentNum);

        if (installment is null)
            return Result.Failure(new Error("Installment.NotFound", "القسط غير موجود"));

        var result = installment.Pay(request.VaultId, request.PaymentMethod);
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
