using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Installments.PayInstallment;

public sealed record PayInstallmentCommand(
    Guid PlanId,
    short InstallmentNum,
    Guid VaultId,
    string PaymentMethod) : IRequest<Result>;
