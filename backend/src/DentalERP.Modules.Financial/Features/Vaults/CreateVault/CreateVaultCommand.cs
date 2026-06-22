using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Vaults.CreateVault;

public sealed record CreateVaultCommand(
    string Name,
    string Type,
    decimal OpeningBalance = 0
) : IRequest<Result<Guid>>;

public sealed class CreateVaultCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateVaultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVaultCommand request, CancellationToken ct)
    {
        if (!Vault.ValidTypes.Contains(request.Type))
            return Result.Failure<Guid>(new Error("Vault.InvalidType",
                $"نوع الخزينة يجب أن يكون: {string.Join(", ", Vault.ValidTypes)}"));

        var vault = Vault.Create(request.Name, request.Type, request.OpeningBalance);
        db.Vaults.Add(vault);
        await db.SaveChangesAsync(ct);
        return Result.Success(vault.Id);
    }
}
