using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.CreatePurchaseOrder;

public sealed record CreatePOItemDto(Guid ItemId, decimal QuantityOrdered, decimal UnitCost, Guid? SupplierItemId, string? Notes);

public sealed record CreatePurchaseOrderCommand(
    Guid SupplierId, DateOnly OrderDate, DateOnly? ExpectedDate,
    decimal DiscountAmount, string? Notes, Guid? CreatedById,
    IReadOnlyList<CreatePOItemDto> Items) : IRequest<Result<Guid>>;

public sealed class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("PO must have at least one item.");
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.QuantityOrdered).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreatePurchaseOrderCommandHandler(PurchasingDbContext db, IPONumberGenerator numGen)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var poNumber = await numGen.GenerateAsync(cancellationToken);
        var po = PurchaseOrder.Create(poNumber, request.SupplierId, request.OrderDate,
            request.ExpectedDate, request.DiscountAmount, request.Notes, request.CreatedById);

        foreach (var item in request.Items)
            po.AddItem(PurchaseOrderItem.Create(po.Id, item.ItemId, item.QuantityOrdered, item.UnitCost, item.SupplierItemId, item.Notes));

        db.PurchaseOrders.Add(po);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(po.Id);
    }
}
