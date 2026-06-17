using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(IAMDbContext db)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.Update(request.FullName, request.Email, request.Phone);
        user.ClearRoles();
        foreach (var roleId in request.RoleIds)
            user.AddRole(roleId);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
