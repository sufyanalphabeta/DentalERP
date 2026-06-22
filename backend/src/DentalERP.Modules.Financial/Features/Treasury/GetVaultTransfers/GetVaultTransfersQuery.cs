using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Treasury.GetVaultTransfers;

public sealed record GetVaultTransfersQuery(int Page = 1, int PageSize = 30)
    : IRequest<Result<VaultTransfersPageDto>>;

public sealed record VaultTransferListDto(
    Guid Id,
    string TransferNumber,
    string FromVaultName,
    string ToVaultName,
    decimal Amount,
    string? Notes,
    DateTime TransferDate
);

public sealed record VaultTransfersPageDto(
    List<VaultTransferListDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public sealed class GetVaultTransfersQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetVaultTransfersQuery, Result<VaultTransfersPageDto>>
{
    public async Task<Result<VaultTransfersPageDto>> Handle(GetVaultTransfersQuery request, CancellationToken ct)
    {
        var total = await db.VaultTransfers.CountAsync(ct);

        var items = await db.VaultTransfers
            .Include(t => t.FromVault)
            .Include(t => t.ToVault)
            .OrderByDescending(t => t.TransferDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new VaultTransferListDto(
                t.Id,
                t.TransferNumber,
                t.FromVault.Name,
                t.ToVault.Name,
                t.Amount,
                t.Notes,
                t.TransferDate
            ))
            .ToListAsync(ct);

        return Result.Success(new VaultTransfersPageDto(items, total, request.Page, request.PageSize));
    }
}
