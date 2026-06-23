using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Permissions;

public sealed class GetPermissionsQueryHandler(IAMDbContext db)
    : IRequestHandler<GetPermissionsQuery, Result<IReadOnlyList<PermissionGroup>>>
{
    public async Task<Result<IReadOnlyList<PermissionGroup>>> Handle(GetPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .ToListAsync(ct);

        var groups = permissions
            .GroupBy(p => p.Module)
            .Select(g => new PermissionGroup(
                g.Key,
                g.OrderBy(p => p.Screen).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                 .Select(p => new PermissionItem(p.Id, p.Name, p.DisplayName, p.Screen)).ToList()
            ))
            .ToList();

        return Result.Success<IReadOnlyList<PermissionGroup>>(groups);
    }
}
