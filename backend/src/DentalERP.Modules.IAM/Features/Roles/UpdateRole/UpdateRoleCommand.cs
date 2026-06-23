using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Roles.UpdateRole;

[RequirePermission("IAM.Roles.Edit")]
public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    IReadOnlyList<Guid> PermissionIds
) : IRequest<Result>;
