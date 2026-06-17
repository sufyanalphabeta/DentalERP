using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Interfaces;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.DeleteUser;

public sealed class DeleteUserCommandHandler(IAMDbContext db, ICurrentUser currentUser)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        if (request.UserId == currentUser.UserId)
            return Result.Failure(new Error("Users.CannotDeleteSelf", "You cannot delete your own account."));

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        user.Delete();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
