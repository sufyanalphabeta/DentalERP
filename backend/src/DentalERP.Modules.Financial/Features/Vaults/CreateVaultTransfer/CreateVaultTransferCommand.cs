using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Vaults.CreateVaultTransfer;

public sealed record CreateVaultTransferCommand(
    Guid FromVaultId,
    Guid ToVaultId,
    decimal Amount,
    string? Notes,
    Guid TransferredById
) : IRequest<Result<Guid>>;
