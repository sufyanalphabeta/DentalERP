using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.ForceChangePassword;

public sealed record ForceChangePasswordCommand(string NewPassword, string ConfirmPassword) : IRequest<Result>;
