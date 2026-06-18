using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyTypes;

public sealed record GetRadiologyTypesQuery(bool ActiveOnly = true) : IRequest<Result<List<RadiologyTypeDto>>>;

public sealed record RadiologyTypeDto(Guid Id, string Name, string? NameAr, decimal BasePrice, bool IsActive);
