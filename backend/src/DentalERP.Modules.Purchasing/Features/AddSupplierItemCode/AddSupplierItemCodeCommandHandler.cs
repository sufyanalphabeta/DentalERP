using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Purchasing.Features.AddSupplierItemCode;

public sealed record AddSupplierItemCodeCommand(
    Guid SupplierId, Guid ItemId, string SupplierItemCode,
    string? SupplierItemName, decimal? LastUnitCost, bool IsPreferred) : IRequest<Result<Guid>>;

public sealed class AddSupplierItemCodeCommandValidator : AbstractValidator<AddSupplierItemCodeCommand>
{
    public AddSupplierItemCodeCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.SupplierItemCode).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddSupplierItemCodeCommandHandler(PurchasingDbContext db)
    : IRequestHandler<AddSupplierItemCodeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddSupplierItemCodeCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate
        var exists = await db.SupplierItems
            .AnyAsync(si => si.SupplierId == request.SupplierId && si.ItemId == request.ItemId, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(Error.Conflict("SupplierItem"));

        var codeExists = await db.SupplierItems
            .AnyAsync(si => si.SupplierId == request.SupplierId && si.SupplierItemCode == request.SupplierItemCode, cancellationToken);
        if (codeExists)
            return Result.Failure<Guid>(new Error("SupplierItem.CodeExists", "Supplier item code already registered."));

        var si = SupplierItem.Create(request.SupplierId, request.ItemId, request.SupplierItemCode,
            request.SupplierItemName, request.LastUnitCost, request.IsPreferred);

        db.SupplierItems.Add(si);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(si.Id);
    }
}
