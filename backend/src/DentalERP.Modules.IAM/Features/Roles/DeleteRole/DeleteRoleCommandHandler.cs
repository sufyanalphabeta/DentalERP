using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(IAMDbContext db)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);
        if (role is null) return Result.Failure(Error.NotFound("Role"));
        if (role.IsSystem) return Result.Failure(new Error("Roles.SystemRole", "System roles cannot be deleted."));

        var inUse = await db.UserRoles.AnyAsync(ur => ur.RoleId == request.RoleId, ct);
        if (inUse) return Result.Failure(new Error("Roles.InUse", "Role is assigned to one or more users."));

        db.Roles.Remove(role);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
