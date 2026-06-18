using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Commissions.PayCommission;

public sealed record PayCommissionCommand(
    Guid CommissionId,
    Guid VaultId,
    Guid? PaidByUserId = null) : IRequest<Result>;
