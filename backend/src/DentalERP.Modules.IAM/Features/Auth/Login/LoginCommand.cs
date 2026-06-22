using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.IAM.Features.Auth.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    Guid UserId,
    string Username,
    string FullName,
    IReadOnlyList<string> Permissions,
    bool MustChangePassword
);
