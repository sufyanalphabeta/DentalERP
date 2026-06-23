using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Roles.GetRoles;

[RequirePermission("IAM.Roles.View")]
public sealed record GetRolesQuery : IRequest<Result<IReadOnlyList<RoleSummary>>>;

public sealed record RoleSummary(Guid Id, string Name, string? Description, bool IsSystem, int PermissionCount);
