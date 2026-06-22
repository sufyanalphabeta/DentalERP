using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Vaults.UpdateVault;

public sealed record UpdateVaultCommand(Guid Id, string Name, string Type) : IRequest<Result>;

public sealed class UpdateVaultCommandHandler(FinancialDbContext db)
    : IRequestHandler<UpdateVaultCommand, Result>
{
    public async Task<Result> Handle(UpdateVaultCommand request, CancellationToken ct)
    {
        if (!Vault.ValidTypes.Contains(request.Type))
            return Result.Failure(new Error("Vault.InvalidType",
                $"نوع الخزينة يجب أن يكون: {string.Join(", ", Vault.ValidTypes)}"));

        var vault = await db.Vaults.FirstOrDefaultAsync(v => v.Id == request.Id, ct);
        if (vault is null)
            return Result.Failure(new Error("Vault.NotFound", "الخزينة غير موجودة"));

        vault.Update(request.Name, request.Type);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
