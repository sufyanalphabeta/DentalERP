using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Features.GetExternalLabs;

public sealed class GetExternalLabsQueryHandler(LaboratoryDbContext db)
    : IRequestHandler<GetExternalLabsQuery, Result<List<ExternalLabDto>>>
{
    public async Task<Result<List<ExternalLabDto>>> Handle(GetExternalLabsQuery request, CancellationToken cancellationToken)
    {
        var query = db.ExternalLabs.AsQueryable();
        if (request.ActiveOnly) query = query.Where(l => l.IsActive);

        var labs = await query
            .OrderBy(l => l.Name)
            .Select(l => new ExternalLabDto(l.Id, l.Name, l.Phone, l.Email, l.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(labs);
    }
}
