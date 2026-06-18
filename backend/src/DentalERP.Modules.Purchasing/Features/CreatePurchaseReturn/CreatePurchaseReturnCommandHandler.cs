using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.CreatePurchaseReturn;

public sealed record ReturnItemDto(Guid ItemId, decimal Quantity, decimal UnitCost, Guid? BatchId);

public sealed record CreatePurchaseReturnCommand(
    Guid SupplierId, DateOnly ReturnDate, string Reason,
    Guid? PoId, string? Notes, Guid? CreatedById,
    IReadOnlyList<ReturnItemDto> Items) : IRequest<Result<Guid>>;

public sealed class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreatePurchaseReturnCommandHandler(PurchasingDbContext db, IReturnNumberGenerator numGen)
    : IRequestHandler<CreatePurchaseReturnCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var returnNumber = await numGen.GenerateAsync(cancellationToken);
        var purchaseReturn = PurchaseReturn.Create(returnNumber, request.SupplierId,
            request.ReturnDate, request.Reason, request.PoId, request.Notes, request.CreatedById);

        foreach (var item in request.Items)
            purchaseReturn.AddItem(PurchaseReturnItem.Create(purchaseReturn.Id, item.ItemId,
                item.Quantity, item.UnitCost, item.BatchId));

        db.PurchaseReturns.Add(purchaseReturn);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(purchaseReturn.Id);
    }
}
