using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.UpdateServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler(FinancialDbContext db)
    : IRequestHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await db.ServiceCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat is null) return Result.Failure(Error.NotFound("ServiceCategory"));
        cat.Update(request.Name, request.SortOrder);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
