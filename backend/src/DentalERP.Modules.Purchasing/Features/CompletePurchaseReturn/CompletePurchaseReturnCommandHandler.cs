using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.CompletePurchaseReturn;

public sealed record CompletePurchaseReturnCommand(Guid ReturnId, Guid? CompletedById) : IRequest<Result>;

public sealed class CompletePurchaseReturnCommandHandler(PurchasingDbContext db)
    : IRequestHandler<CompletePurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(CompletePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure(Error.NotFound("PurchaseReturn"));

        var result = ret.Complete();
        if (result.IsFailure) return result;

        db.AuditLogs.Add(new AuditLogEntry
        {
            EntityType = "PurchaseReturn",
            EntityId   = ret.Id,
            Action     = "PurchaseReturn.Completed",
            PerformedById = request.CompletedById,
            Details    = $"Return {ret.ReturnNumber} completed. Total: {ret.TotalAmount}"
        });

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
