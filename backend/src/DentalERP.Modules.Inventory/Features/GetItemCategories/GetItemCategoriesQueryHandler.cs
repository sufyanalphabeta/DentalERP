using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetItemCategories;

public sealed record GetItemCategoriesQuery() : IRequest<Result<IReadOnlyList<CategoryDto>>>;
public sealed record CategoryDto(Guid Id, string Name, string? NameAr, Guid? ParentId, bool IsActive);

public sealed class GetItemCategoriesQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetItemCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetItemCategoriesQuery request, CancellationToken cancellationToken)
    {
        var result = await db.ItemCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.NameAr, c.ParentId, c.IsActive))
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<CategoryDto>>(result);
    }
}
