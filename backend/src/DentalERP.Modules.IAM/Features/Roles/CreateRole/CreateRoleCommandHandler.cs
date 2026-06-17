using DentalERP.Modules.IAM.Domain.Entities;
using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Roles.CreateRole;

public sealed class CreateRoleCommandHandler(IAMDbContext db)
    : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(r => r.Name == request.Name, ct))
            return Result.Failure<Guid>(Error.Conflict("Role"));

        var role = Role.Create(request.Name, request.Description);
        foreach (var permId in request.PermissionIds)
            role.AddPermission(permId);

        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);
        return Result.Success(role.Id);
    }
}
