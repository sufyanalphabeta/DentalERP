using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Treasury.GetVaultMovements;

public sealed record GetVaultMovementsQuery(
    Guid? VaultId = null,
    string? Direction = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int Page = 1,
    int PageSize = 30
) : IRequest<Result<VaultMovementsPageDto>>;

public sealed record VaultMovementDto(
    Guid Id,
    string VaultName,
    string Direction,
    decimal Amount,
    string TransactionType,
    string? ReferenceNumber,
    string? Notes,
    DateTime CreatedAt
);

public sealed record VaultMovementsPageDto(
    List<VaultMovementDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public sealed class GetVaultMovementsQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetVaultMovementsQuery, Result<VaultMovementsPageDto>>
{
    public async Task<Result<VaultMovementsPageDto>> Handle(GetVaultMovementsQuery request, CancellationToken ct)
    {
        var q = db.VaultTransactions
            .Where(t => !t.IsReversed && !t.IsReversal)
            .Join(db.Vaults, t => t.VaultId, v => v.Id, (t, v) => new { Tx = t, VaultName = v.Name });

        if (request.VaultId.HasValue)
            q = q.Where(x => x.Tx.VaultId == request.VaultId.Value);

        if (!string.IsNullOrEmpty(request.Direction))
        {
            var dir = request.Direction.ToLower();
            q = q.Where(x => x.Tx.Direction == dir);
        }

        if (request.DateFrom.HasValue)
            q = q.Where(x => x.Tx.CreatedAt >= DateTime.SpecifyKind(request.DateFrom.Value, DateTimeKind.Utc));

        if (request.DateTo.HasValue)
            q = q.Where(x => x.Tx.CreatedAt < DateTime.SpecifyKind(request.DateTo.Value.AddDays(1), DateTimeKind.Utc));

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.Tx.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new VaultMovementDto(
                x.Tx.Id,
                x.VaultName,
                x.Tx.Direction == "in" ? "In" : "Out",
                x.Tx.Amount,
                x.Tx.TransactionType,
                x.Tx.ReferenceNumber,
                x.Tx.Notes,
                x.Tx.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Success(new VaultMovementsPageDto(items, total, request.Page, request.PageSize));
    }
}
