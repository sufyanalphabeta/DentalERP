using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CreateExternalLab;

public sealed record CreateExternalLabCommand(
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Address,
    string? Notes
) : IRequest<Result<Guid>>;
