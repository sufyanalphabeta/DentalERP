using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Roles.GetRole;

[RequirePermission("IAM.Roles.View")]
public sealed record GetRoleQuery(Guid RoleId) : IRequest<Result<RoleDetail>>;

public sealed record RoleDetail(
    Guid Id, string Name, string? Description, bool IsSystem,
    IReadOnlyList<PermissionInfo> Permissions);

public sealed record PermissionInfo(Guid Id, string Name, string DisplayName, string Module, string? Screen);
