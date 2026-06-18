using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.CreateWarehouse;

public sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateWarehouseCommandHandler(InventoryDbContext db)
    : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = Warehouse.Create(request.Name, request.NameAr, request.Location, request.IsDefault);
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(warehouse.Id);
    }
}
