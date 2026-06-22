using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.CreateServiceCategory;

public sealed class CreateServiceCategoryCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateServiceCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = ServiceCategory.Create(request.Name, request.SortOrder);
        db.ServiceCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }
}
