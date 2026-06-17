using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.DeleteUser;

[RequirePermission("Users.Delete")]
public sealed record DeleteUserCommand(Guid UserId) : IRequest<Result>;
