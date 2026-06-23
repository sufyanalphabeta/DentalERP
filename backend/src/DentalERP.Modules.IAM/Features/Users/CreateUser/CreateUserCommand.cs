using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.CreateUser;

[RequirePermission("IAM.Users.Create")]
public sealed record CreateUserCommand(
    string Username,
    string FullName,
    string? Email,
    string? Phone,
    IReadOnlyList<Guid> RoleIds
) : IRequest<Result<Guid>>;
