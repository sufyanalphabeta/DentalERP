using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.GetUsersList;

public sealed record GetUsersListQuery : IRequest<Result<IReadOnlyList<UserListItem>>>;

public sealed record UserListItem(Guid Id, string Username, string FullName);
