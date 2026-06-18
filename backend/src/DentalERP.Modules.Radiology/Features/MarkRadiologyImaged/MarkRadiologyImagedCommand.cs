using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.MarkRadiologyImaged;

public sealed record MarkRadiologyImagedCommand(Guid OrderId) : IRequest<Result>;
