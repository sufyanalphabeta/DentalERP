using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.GetUsers;

[RequirePermission("Users.View")]
public sealed record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<Result<GetUsersResponse>>;

public sealed record UserSummary(
    Guid Id, string Username, string FullName, string? Email, string? Phone,
    bool IsActive, DateTime? LastLoginAt, IReadOnlyList<string> Roles);

public sealed record GetUsersResponse(IReadOnlyList<UserSummary> Items, int TotalCount, int Page, int PageSize);
