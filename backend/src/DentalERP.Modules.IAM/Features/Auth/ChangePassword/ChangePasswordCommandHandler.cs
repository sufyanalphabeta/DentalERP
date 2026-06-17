using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler(IAMDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure(new Error("Auth.WrongPassword", "Current password is incorrect."));

        if (request.NewPassword != request.ConfirmPassword)
            return Result.Failure(new Error("Auth.PasswordMismatch", "New password and confirmation do not match."));

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
