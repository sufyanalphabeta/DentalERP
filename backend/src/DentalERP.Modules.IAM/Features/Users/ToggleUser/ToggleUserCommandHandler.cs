using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.ToggleUser;

public sealed class ToggleUserCommandHandler(IAMDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ToggleUserCommand, Result>
{
    public async Task<Result> Handle(ToggleUserCommand request, CancellationToken ct)
    {
        if (request.UserId == currentUser.UserId)
            return Result.Failure(new Error("Users.CannotDisableSelf", "You cannot disable your own account."));

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.SetActive(request.IsActive);
        if (!request.IsActive) user.RevokeAllRefreshTokens();

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
