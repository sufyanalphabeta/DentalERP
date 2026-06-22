using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.ResetPassword;

public sealed class ResetPasswordCommandHandler(IAMDbContext db)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    private const string DefaultPassword = "123456";

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(DefaultPassword), clearMustChange: false);
        user.SetMustChangePassword(true);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
