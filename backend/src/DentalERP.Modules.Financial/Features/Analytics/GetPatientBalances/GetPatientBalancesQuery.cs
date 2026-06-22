using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Analytics.GetPatientBalances;

public sealed record GetPatientBalancesQuery() : IRequest<Result<List<PatientBalanceDto>>>;

public sealed record PatientBalanceDto(Guid PatientId, decimal Outstanding);
