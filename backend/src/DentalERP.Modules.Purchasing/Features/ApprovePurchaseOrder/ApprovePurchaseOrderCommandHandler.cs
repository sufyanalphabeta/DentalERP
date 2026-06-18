using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.ApprovePurchaseOrder;

public sealed record ApprovePurchaseOrderCommand(Guid PoId, Guid ApprovedById) : IRequest<Result>;

public sealed class ApprovePurchaseOrderCommandValidator : AbstractValidator<ApprovePurchaseOrderCommand>
{
    public ApprovePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.ApprovedById).NotEmpty();
    }
}

public sealed class ApprovePurchaseOrderCommandHandler(PurchasingDbContext db)
    : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == request.PoId, cancellationToken);

        if (po is null) return Result.Failure(Error.NotFound("PurchaseOrder"));

        var result = po.Approve(request.ApprovedById);
        if (result.IsFailure) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
