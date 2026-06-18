using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.MarkPOSent;

public sealed record MarkPOSentCommand(Guid PoId) : IRequest<Result>;

public sealed class MarkPOSentCommandHandler(PurchasingDbContext db)
    : IRequestHandler<MarkPOSentCommand, Result>
{
    public async Task<Result> Handle(MarkPOSentCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders.FindAsync([request.PoId], cancellationToken);
        if (po is null) return Result.Failure(Error.NotFound("PurchaseOrder"));
        var result = po.MarkSent();
        if (result.IsFailure) return result;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
