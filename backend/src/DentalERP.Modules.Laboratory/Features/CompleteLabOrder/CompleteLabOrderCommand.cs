using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CompleteLabOrder;

public sealed record CompleteLabOrderCommand(Guid OrderId) : IRequest<Result>;
