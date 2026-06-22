using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.ToggleServiceCategory;

public sealed class ToggleServiceCategoryCommandHandler(FinancialDbContext db)
    : IRequestHandler<ToggleServiceCategoryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await db.ServiceCategories.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat is null) return Result.Failure<bool>(Error.NotFound("ServiceCategory"));

        if (cat.IsActive) cat.Deactivate(); else cat.Activate();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(cat.IsActive);
    }
}
