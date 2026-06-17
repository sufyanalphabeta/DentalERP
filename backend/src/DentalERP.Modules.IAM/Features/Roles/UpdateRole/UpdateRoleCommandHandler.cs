using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(IAMDbContext db)
    : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role is null) return Result.Failure(Error.NotFound("Role"));
        if (role.IsSystem) return Result.Failure(new Error("Roles.SystemRole", "System roles cannot be modified."));

        if (await db.Roles.AnyAsync(r => r.Name == request.Name && r.Id != request.RoleId, ct))
            return Result.Failure(Error.Conflict("Role"));

        role.Update(request.Name, request.Description);
        role.ClearPermissions();
        foreach (var permId in request.PermissionIds)
            role.AddPermission(permId);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
