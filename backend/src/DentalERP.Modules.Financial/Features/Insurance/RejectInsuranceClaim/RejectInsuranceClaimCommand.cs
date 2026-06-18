using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.RejectInsuranceClaim;

public sealed record RejectInsuranceClaimCommand(Guid ClaimId, string Reason) : IRequest<Result>;
