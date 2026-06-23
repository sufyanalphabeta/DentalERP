using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Roles.GetRole;

public sealed class GetRoleQueryHandler(IAMDbContext db)
    : IRequestHandler<GetRoleQuery, Result<RoleDetail>>
{
    public async Task<Result<RoleDetail>> Handle(GetRoleQuery request, CancellationToken ct)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role is null) return Result.Failure<RoleDetail>(Error.NotFound("Role"));

        return Result.Success(new RoleDetail(
            role.Id, role.Name, role.Description, role.IsSystem,
            role.RolePermissions
                .Select(rp => new PermissionInfo(rp.Permission.Id, rp.Permission.Name, rp.Permission.DisplayName, rp.Permission.Module, rp.Permission.Screen))
                .ToList()
        ));
    }
}
