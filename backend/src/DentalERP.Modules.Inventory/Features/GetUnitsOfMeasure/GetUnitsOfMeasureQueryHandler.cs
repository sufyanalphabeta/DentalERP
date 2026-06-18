using DentalERP.Modules.Inventory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Inventory.Features.GetUnitsOfMeasure;

public sealed record GetUnitsOfMeasureQuery() : IRequest<Result<IReadOnlyList<UomDto>>>;
public sealed record UomDto(Guid Id, string Name, string? NameAr, string? Abbreviation);

public sealed class GetUnitsOfMeasureQueryHandler(InventoryDbContext db)
    : IRequestHandler<GetUnitsOfMeasureQuery, Result<IReadOnlyList<UomDto>>>
{
    public async Task<Result<IReadOnlyList<UomDto>>> Handle(GetUnitsOfMeasureQuery request, CancellationToken cancellationToken)
    {
        var result = await db.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UomDto(u.Id, u.Name, u.NameAr, u.Abbreviation))
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<UomDto>>(result);
    }
}
