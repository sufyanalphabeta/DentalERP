using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.ToggleUser;

[RequirePermission("IAM.Users.Edit")]
public sealed record ToggleUserCommand(Guid UserId, bool IsActive) : IRequest<Result>;
