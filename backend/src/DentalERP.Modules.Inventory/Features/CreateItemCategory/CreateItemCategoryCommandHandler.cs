using DentalERP.Modules.Inventory.Domain.Entities;
using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.CreateItemCategory;

public sealed record CreateItemCategoryCommand(string Name, string? NameAr, Guid? ParentId) : IRequest<Result<Guid>>;

public sealed class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
{
    public CreateItemCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CreateItemCategoryCommandHandler(InventoryDbContext db)
    : IRequestHandler<CreateItemCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = ItemCategory.Create(request.Name, request.NameAr, request.ParentId);
        db.ItemCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }
}
