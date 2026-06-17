using DentalERP.Modules.IAM.Domain.Entities;
using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Users.CreateUser;

public sealed class CreateUserCommandHandler(IAMDbContext db)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username.ToLowerInvariant(), ct))
            return Result.Failure<Guid>(Error.Conflict("Username"));

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Username, hash, request.FullName, request.Email, request.Phone);

        foreach (var roleId in request.RoleIds)
            user.AddRole(roleId);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return Result.Success(user.Id);
    }
}
