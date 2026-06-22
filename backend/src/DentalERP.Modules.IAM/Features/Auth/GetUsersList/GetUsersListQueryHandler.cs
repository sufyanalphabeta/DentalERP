using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.GetUsersList;

public sealed class GetUsersListQueryHandler(IAMDbContext db)
    : IRequestHandler<GetUsersListQuery, Result<IReadOnlyList<UserListItem>>>
{
    public async Task<Result<IReadOnlyList<UserListItem>>> Handle(GetUsersListQuery request, CancellationToken ct)
    {
        var users = await db.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new UserListItem(u.Id, u.Username, u.FullName))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<UserListItem>>(users);
    }
}
