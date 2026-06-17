using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.GetUsers;

public sealed class GetUsersQueryHandler(IAMDbContext db)
    : IRequestHandler<GetUsersQuery, Result<GetUsersResponse>>
{
    public async Task<Result<GetUsersResponse>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var query = db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(u =>
                u.Username.Contains(s) ||
                u.FullName.ToLower().Contains(s) ||
                (u.Email != null && u.Email.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = users.Select(u => new UserSummary(
            u.Id, u.Username, u.FullName, u.Email, u.Phone,
            u.IsActive, u.LastLoginAt,
            u.UserRoles.Select(ur => ur.Role.Name).ToList()
        )).ToList();

        return Result.Success(new GetUsersResponse(items, total, request.Page, request.PageSize));
    }
}
