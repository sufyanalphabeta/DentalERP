using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword)
    : IRequest<Result>;
