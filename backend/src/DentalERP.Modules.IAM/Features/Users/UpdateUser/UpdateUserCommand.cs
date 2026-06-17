using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.UpdateUser;

[RequirePermission("Users.Edit")]
public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string? Email,
    string? Phone,
    IReadOnlyList<Guid> RoleIds
) : IRequest<Result>;
