using DentalERP.SharedKernel.Results;
using DentalERP.Modules.IAM.Features.Auth.Login;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;
