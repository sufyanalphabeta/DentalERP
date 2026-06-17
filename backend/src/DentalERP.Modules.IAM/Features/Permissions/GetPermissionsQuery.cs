using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Permissions;

[RequirePermission("Roles.View")]
public sealed record GetPermissionsQuery : IRequest<Result<IReadOnlyList<PermissionGroup>>>;

public sealed record PermissionItem(Guid Id, string Name, string DisplayName);
public sealed record PermissionGroup(string Module, IReadOnlyList<PermissionItem> Permissions);
