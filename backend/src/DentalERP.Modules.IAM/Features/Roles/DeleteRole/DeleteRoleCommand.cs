using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Roles.DeleteRole;

[RequirePermission("Roles.Delete")]
public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;
