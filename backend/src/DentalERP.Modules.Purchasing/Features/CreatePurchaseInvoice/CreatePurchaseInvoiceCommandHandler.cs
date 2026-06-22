using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.CreatePurchaseInvoice;

public sealed record PILineDto(
    Guid ItemId, string ItemName, string? ItemCode, string? Barcode, string? UnitName,
    decimal Quantity, decimal PurchasePrice, decimal? SalePrice,
    DateOnly? ExpiryDate, string? BatchNumber, int SortOrder = 0);

public sealed record CreatePurchaseInvoiceCommand(
    Guid SupplierId, DateOnly InvoiceDate, Guid? WarehouseId,
    decimal Discount, string? Notes, Guid? CreatedById,
    IReadOnlyList<PILineDto> Items) : IRequest<Result<Guid>>;

public sealed class CreatePurchaseInvoiceCommandValidator : AbstractValidator<CreatePurchaseInvoiceCommand>
{
    public CreatePurchaseInvoiceCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ItemId).NotEmpty();
            i.RuleFor(x => x.Quantity).GreaterThan(0);
            i.RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreatePurchaseInvoiceCommandHandler(PurchasingDbContext db, IPINumberGenerator numGen)
    : IRequestHandler<CreatePurchaseInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var number = await numGen.GenerateAsync(cancellationToken);
        var inv = PurchaseInvoice.Create(number, request.SupplierId, request.InvoiceDate,
            request.WarehouseId, request.Notes, request.CreatedById);

        for (int i = 0; i < request.Items.Count; i++)
        {
            var dto = request.Items[i];
            var line = PurchaseInvoiceItem.Create(inv.Id, dto.ItemId, dto.ItemName, dto.Quantity,
                dto.PurchasePrice, dto.ItemCode, dto.Barcode, dto.UnitName, dto.SalePrice,
                dto.ExpiryDate, dto.BatchNumber, dto.SortOrder == 0 ? i : dto.SortOrder);
            inv.AddItem(line);
        }

        // Apply discount after items are added (RecalcTotals already ran per AddItem)
        inv.UpdateHeader(request.InvoiceDate, request.WarehouseId, request.Discount, request.Notes);

        db.PurchaseInvoices.Add(inv);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(inv.Id);
    }
}
