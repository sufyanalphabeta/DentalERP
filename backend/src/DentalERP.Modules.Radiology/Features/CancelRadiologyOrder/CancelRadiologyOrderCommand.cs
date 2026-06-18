using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.CancelRadiologyOrder;

public sealed record CancelRadiologyOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;
