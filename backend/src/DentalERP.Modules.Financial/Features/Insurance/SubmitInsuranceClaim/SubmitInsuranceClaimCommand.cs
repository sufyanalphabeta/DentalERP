using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Insurance.SubmitInsuranceClaim;

public sealed record SubmitInsuranceClaimCommand(Guid ClaimId) : IRequest<Result>;
