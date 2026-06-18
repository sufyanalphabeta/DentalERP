using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.SendLabOrder;

public sealed record SendLabOrderCommand(Guid OrderId) : IRequest<Result>;
