using DentalERP.Modules.IAM.Domain.Entities;
using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.ManageUserPermissions;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public sealed record UserPermissionDto(
    Guid PermissionId,
    string Name,
    string DisplayName,
    string Module,
    string? Screen,
    string GrantType);

public sealed record EffectivePermissionsDto(
    IReadOnlyList<UserPermissionDto> RolePermissions,
    IReadOnlyList<UserPermissionDto> AdditionalGrants,
    IReadOnlyList<UserPermissionDto> ExplicitDenies,
    IReadOnlyList<string> Effective);

// ── Queries / Commands ────────────────────────────────────────────────────────

public sealed record GetUserEffectivePermissionsQuery(Guid UserId)
    : IRequest<Result<EffectivePermissionsDto>>;

public sealed record SetUserPermissionsCommand(
    Guid UserId,
    IReadOnlyList<UserPermissionOverrideDto> Overrides)
    : IRequest<Result>;

public sealed record UserPermissionOverrideDto(Guid PermissionId, string GrantType);

// ── GetUserEffectivePermissions Handler ───────────────────────────────────────

public sealed class GetUserEffectivePermissionsHandler(IAMDbContext db)
    : IRequestHandler<GetUserEffectivePermissionsQuery, Result<EffectivePermissionsDto>>
{
    public async Task<Result<EffectivePermissionsDto>> Handle(
        GetUserEffectivePermissionsQuery request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
            return Result.Failure<EffectivePermissionsDto>(new Error("User.NotFound", "User not found."));

        var rolePerms = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => new UserPermissionDto(
                rp.PermissionId, rp.Permission.Name, rp.Permission.DisplayName,
                rp.Permission.Module, rp.Permission.Screen, "Grant"))
            .DistinctBy(p => p.PermissionId)
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .ToList();

        var grants = user.UserPermissions
            .Where(up => up.GrantType == "Grant")
            .Select(up => new UserPermissionDto(
                up.PermissionId, up.Permission.Name, up.Permission.DisplayName,
                up.Permission.Module, up.Permission.Screen, "Grant"))
            .ToList();

        var denies = user.UserPermissions
            .Where(up => up.GrantType == "Deny")
            .Select(up => new UserPermissionDto(
                up.PermissionId, up.Permission.Name, up.Permission.DisplayName,
                up.Permission.Module, up.Permission.Screen, "Deny"))
            .ToList();

        var denyNames = denies.Select(d => d.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var effective = rolePerms.Select(p => p.Name)
            .Union(grants.Select(g => g.Name))
            .Where(n => !denyNames.Contains(n))
            .Distinct()
            .Order()
            .ToList();

        return Result.Success(new EffectivePermissionsDto(rolePerms, grants, denies, effective));
    }
}

// ── SetUserPermissions Handler ────────────────────────────────────────────────

public sealed class SetUserPermissionsHandler(IAMDbContext db)
    : IRequestHandler<SetUserPermissionsCommand, Result>
{
    public async Task<Result> Handle(SetUserPermissionsCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
            return Result.Failure(new Error("User.NotFound", "User not found."));

        // Remove all existing overrides
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM user_permissions WHERE user_id = {0}", request.UserId);

        // Re-insert from request
        if (request.Overrides.Count > 0)
        {
            var newOverrides = request.Overrides
                .Where(o => o.GrantType is "Grant" or "Deny")
                .Select(o => new UserPermission
                {
                    UserId = request.UserId,
                    PermissionId = o.PermissionId,
                    GrantType = o.GrantType
                })
                .ToList();

            db.UserPermissions.AddRange(newOverrides);
            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
