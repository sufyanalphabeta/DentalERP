using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.Logout;

public sealed class LogoutCommandHandler(IAMDbContext db)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (token is null) return Result.Success();

        token.User.RevokeRefreshToken(request.RefreshToken);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
