using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.GetUser;

[RequirePermission("IAM.Users.View")]
public sealed record GetUserQuery(Guid UserId) : IRequest<Result<UserDetail>>;

public sealed record UserDetail(
    Guid Id, string Username, string FullName, string? Email, string? Phone,
    bool IsActive, DateTime? LastLoginAt, DateTime CreatedAt,
    IReadOnlyList<RoleInfo> Roles, IReadOnlyList<string> Permissions);

public sealed record RoleInfo(Guid Id, string Name);
