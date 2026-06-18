using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.CompleteRadiologyOrder;

public sealed record CompleteRadiologyOrderCommand(Guid OrderId) : IRequest<Result>;
