using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.GetExternalLabs;

public sealed record GetExternalLabsQuery(bool ActiveOnly = true) : IRequest<Result<List<ExternalLabDto>>>;

public sealed record ExternalLabDto(Guid Id, string Name, string? Phone, string? Email, bool IsActive);
