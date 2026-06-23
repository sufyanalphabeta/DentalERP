using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Roles.CreateRole;

[RequirePermission("IAM.Roles.Create")]
public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    IReadOnlyList<Guid> PermissionIds
) : IRequest<Result<Guid>>;
