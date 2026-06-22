using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetCollectionSummary;

public sealed record GetCollectionSummaryQuery(DateOnly From, DateOnly To) : IRequest<Result<CollectionSummaryDto>>;

public sealed record CollectionSummaryDto(
    DateOnly From, DateOnly To,
    decimal TotalCollected,
    IReadOnlyList<VaultCollectionDto> ByVault,
    IReadOnlyList<MethodCollectionDto> ByMethod,
    IReadOnlyList<DailyCollectionDto> Daily);

public sealed record VaultCollectionDto(Guid VaultId, string VaultName, decimal Amount);
public sealed record MethodCollectionDto(string Method, string MethodAr, decimal Amount);
public sealed record DailyCollectionDto(DateOnly Date, decimal Amount, int TransactionCount);

internal sealed class GetCollectionSummaryQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetCollectionSummaryQuery, Result<CollectionSummaryDto>>
{
    public async Task<Result<CollectionSummaryDto>> Handle(GetCollectionSummaryQuery request, CancellationToken ct)
    {
        var fromUtc = new DateTime(request.From.Year, request.From.Month, request.From.Day, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(request.To.Year, request.To.Month, request.To.Day, 23, 59, 59, DateTimeKind.Utc);

        var payments = await db.Payments
            .Where(p => p.CreatedAt >= fromUtc && p.CreatedAt <= toUtc)
            .Select(p => new { p.VaultId, p.PaymentMethod, p.Amount, p.CreatedAt })
            .ToListAsync(ct);

        var vaultIds = payments.Select(p => p.VaultId).Distinct().ToList();
        var vaultNames = await db.Vaults
            .Where(v => vaultIds.Contains(v.Id))
            .Select(v => new { v.Id, v.Name })
            .ToDictionaryAsync(v => v.Id, v => v.Name, ct);

        var methodAr = (string m) => m switch
        {
            "cash" => "نقدي",
            "bank_transfer" => "تحويل بنكي",
            "card" => "بطاقة",
            "pos" => "POS",
            "cheque" => "شيك",
            "insurance" => "تأمين",
            _ => m
        };

        var byVault = payments
            .GroupBy(p => p.VaultId)
            .Select(g => new VaultCollectionDto(g.Key, vaultNames.GetValueOrDefault(g.Key, "—"), g.Sum(p => p.Amount)))
            .OrderByDescending(v => v.Amount)
            .ToList();

        var byMethod = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new MethodCollectionDto(g.Key, methodAr(g.Key), g.Sum(p => p.Amount)))
            .OrderByDescending(m => m.Amount)
            .ToList();

        var daily = payments
            .GroupBy(p => DateOnly.FromDateTime(p.CreatedAt))
            .Select(g => new DailyCollectionDto(g.Key, g.Sum(p => p.Amount), g.Count()))
            .OrderBy(d => d.Date)
            .ToList();

        var total = payments.Sum(p => p.Amount);

        return Result.Success(new CollectionSummaryDto(request.From, request.To, total, byVault, byMethod, daily));
    }
}
