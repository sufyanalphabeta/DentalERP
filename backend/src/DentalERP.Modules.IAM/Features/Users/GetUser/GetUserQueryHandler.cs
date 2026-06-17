using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.GetUser;

public sealed class GetUserQueryHandler(IAMDbContext db)
    : IRequestHandler<GetUserQuery, Result<UserDetail>>
{
    public async Task<Result<UserDetail>> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null) return Result.Failure<UserDetail>(Error.NotFound("User"));

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        return Result.Success(new UserDetail(
            user.Id, user.Username, user.FullName, user.Email, user.Phone,
            user.IsActive, user.LastLoginAt, user.CreatedAt,
            user.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList(),
            permissions
        ));
    }
}
