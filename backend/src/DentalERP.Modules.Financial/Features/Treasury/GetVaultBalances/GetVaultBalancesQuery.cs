using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Treasury.GetVaultBalances;

public sealed record GetVaultBalancesQuery : IRequest<Result<List<VaultBalanceDto>>>;

public sealed record VaultBalanceDto(
    Guid Id,
    string Name,
    string Type,
    decimal OpeningBalance,
    decimal TotalIn,
    decimal TotalOut,
    decimal CurrentBalance);
