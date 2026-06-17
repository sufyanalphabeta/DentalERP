using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
