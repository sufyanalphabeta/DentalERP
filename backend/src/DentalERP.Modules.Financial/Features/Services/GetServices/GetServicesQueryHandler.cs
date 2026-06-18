using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.GetServices;

public sealed class GetServicesQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetServicesQuery, Result<List<ServiceDto>>>
{
    public async Task<Result<List<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await db.MedicalServices
            .Where(s => request.ActiveOnly ? s.IsActive : true)
            .Where(s => string.IsNullOrWhiteSpace(request.Search) ||
                        s.Name.Contains(request.Search) ||
                        (s.Code != null && s.Code.Contains(request.Search)))
            .Where(s => !request.CategoryId.HasValue || s.CategoryId == request.CategoryId)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(
                s.Id,
                s.Name,
                s.Code,
                s.Price,
                s.CategoryId,
                db.ServiceCategories.Where(c => c.Id == s.CategoryId).Select(c => c.Name).FirstOrDefault(),
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(services);
    }
}
