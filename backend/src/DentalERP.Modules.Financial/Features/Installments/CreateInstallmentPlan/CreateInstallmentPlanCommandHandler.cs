using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Installments.CreateInstallmentPlan;

public sealed class CreateInstallmentPlanCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateInstallmentPlanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInstallmentPlanCommand request, CancellationToken cancellationToken)
    {
        if (request.TotalAmount <= 0)
            return Result.Failure<Guid>(new Error("Installment.InvalidAmount", "المبلغ الإجمالي يجب أن يكون أكبر من صفر"));
        if (request.InstallmentsCount <= 0)
            return Result.Failure<Guid>(new Error("Installment.InvalidCount", "عدد الأقساط يجب أن يكون أكبر من صفر"));

        var plan = InstallmentPlan.Create(
            request.InvoiceId,
            request.PatientId,
            request.TotalAmount,
            request.InstallmentsCount,
            request.StartDate,
            request.Notes,
            request.CreatedByUserId);

        db.InstallmentPlans.Add(plan);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(plan.Id);
    }
}
