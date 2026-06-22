using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Auth.ForceChangePassword;

public sealed class ForceChangePasswordCommandHandler(IAMDbContext db, ICurrentUser currentUser)
    : IRequestHandler<ForceChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ForceChangePasswordCommand request, CancellationToken ct)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Result.Failure(new Error("Auth.PasswordMismatch", "كلمة المرور وتأكيدها غير متطابقتين."));

        if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"^\d{4,8}$"))
            return Result.Failure(new Error("Auth.InvalidPassword", "كلمة المرور يجب أن تكون 4 إلى 8 أرقام فقط."));

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword), clearMustChange: true);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
