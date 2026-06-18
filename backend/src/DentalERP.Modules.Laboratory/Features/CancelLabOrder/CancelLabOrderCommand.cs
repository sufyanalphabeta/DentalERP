using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CancelLabOrder;

public sealed record CancelLabOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;
