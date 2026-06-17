using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Roles.GetRoles;

public sealed class GetRolesQueryHandler(IAMDbContext db)
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleSummary>>>
{
    public async Task<Result<IReadOnlyList<RoleSummary>>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var roles = await db.Roles
            .Include(r => r.RolePermissions)
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleSummary(r.Id, r.Name, r.Description, r.IsSystem, r.RolePermissions.Count))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<RoleSummary>>(roles);
    }
}
