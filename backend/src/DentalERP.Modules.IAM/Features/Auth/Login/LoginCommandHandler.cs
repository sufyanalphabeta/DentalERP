using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.Modules.IAM.Infrastructure.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.Login;

public sealed class LoginCommandHandler(IAMDbContext db, JwtService jwtService, AuditService auditService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == request.Username.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(new Error("Auth.InvalidCredentials", "Username or password is incorrect."));

        if (!user.IsActive)
            return Result.Failure<LoginResponse>(new Error("Auth.AccountDisabled", "Your account has been disabled."));

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Username, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshExpiry = jwtService.RefreshTokenExpiry();

        // Update last_login_at via raw SQL to avoid EF Core batch concurrency issues
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE users SET last_login_at = {0}, updated_at = {1} WHERE id = {2}",
            DateTime.UtcNow, DateTime.UtcNow, user.Id);

        // Revoke existing refresh tokens
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE refresh_tokens SET revoked_at = {0} WHERE user_id = {1} AND revoked_at IS NULL",
            DateTime.UtcNow, user.Id);

        // Insert new refresh token and audit log
        db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiry
        };
        db.Set<Domain.Entities.RefreshToken>().Add(newRefreshToken);
        db.AuditLogs.Add(auditService.CreateActionLog("Login", "User", user.Id.ToString()));

        await db.SaveChangesAsync(ct);

        return Result.Success(new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            user.Id,
            user.Username,
            user.FullName,
            permissions,
            user.MustChangePassword
        ));
    }
}
