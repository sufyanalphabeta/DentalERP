using DentalERP.Modules.IAM.Features.Auth.Login;
using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.Modules.IAM.Infrastructure.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(IAMDbContext db, JwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (existing is null || !existing.IsActive)
            return Result.Failure<LoginResponse>(new Error("Auth.InvalidRefreshToken", "Refresh token is invalid or expired."));

        var user = existing.User;
        if (!user.IsActive)
            return Result.Failure<LoginResponse>(new Error("Auth.AccountDisabled", "Account is disabled."));

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Username, permissions);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var refreshExpiry = jwtService.RefreshTokenExpiry();

        user.RevokeRefreshToken(request.RefreshToken);
        user.AddRefreshToken(newRefreshToken, refreshExpiry);

        await db.SaveChangesAsync(ct);

        return Result.Success(new LoginResponse(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(60),
            user.Id,
            user.Username,
            user.FullName,
            permissions
        ));
    }
}
