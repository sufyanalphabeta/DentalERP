using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.CreateInsuranceClaim;

public sealed record CreateInsuranceClaimCommand(
    Guid InvoiceId,
    Guid InsuranceCompanyId,
    Guid PatientId,
    decimal ClaimedAmount,
    decimal CoveragePercent,
    string? Notes
) : IRequest<Result<Guid>>;
