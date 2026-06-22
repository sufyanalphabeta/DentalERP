using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.GetServiceCategories;

public sealed class GetServiceCategoriesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetServiceCategoriesQuery, Result<List<ServiceCategoryDto>>>
{
    public async Task<Result<List<ServiceCategoryDto>>> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.ServiceCategories
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ServiceCategoryDto(c.Id, c.Name, c.SortOrder, c.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(categories);
    }
}
