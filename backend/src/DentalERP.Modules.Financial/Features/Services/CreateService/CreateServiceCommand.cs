using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.CreateService;

public sealed record CreateServiceCommand(
    string Name,
    decimal Price,
    Guid? CategoryId = null,
    string? Code = null) : IRequest<Result<Guid>>;
