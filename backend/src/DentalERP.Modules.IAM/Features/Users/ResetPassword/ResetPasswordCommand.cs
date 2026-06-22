using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Users.ResetPassword;

[RequirePermission("Users.Edit")]
public sealed record ResetPasswordCommand(Guid UserId) : IRequest<Result>;
