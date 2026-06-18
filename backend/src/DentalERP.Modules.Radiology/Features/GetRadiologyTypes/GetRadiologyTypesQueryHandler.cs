using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyTypes;

public sealed class GetRadiologyTypesQueryHandler(RadiologyDbContext db)
    : IRequestHandler<GetRadiologyTypesQuery, Result<List<RadiologyTypeDto>>>
{
    public async Task<Result<List<RadiologyTypeDto>>> Handle(GetRadiologyTypesQuery request, CancellationToken cancellationToken)
    {
        var query = db.RadiologyTypes.AsQueryable();
        if (request.ActiveOnly) query = query.Where(t => t.IsActive);

        var types = await query
            .OrderBy(t => t.Name)
            .Select(t => new RadiologyTypeDto(t.Id, t.Name, t.NameAr, t.BasePrice, t.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(types);
    }
}
