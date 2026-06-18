using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    decimal Price,
    Guid? CategoryId = null,
    string? Code = null) : IRequest<Result>;
