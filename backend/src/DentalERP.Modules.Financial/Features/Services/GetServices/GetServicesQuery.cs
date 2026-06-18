using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.GetServices;

public sealed record GetServicesQuery(string? Search = null, Guid? CategoryId = null, bool ActiveOnly = true)
    : IRequest<Result<List<ServiceDto>>>;

public sealed record ServiceDto(
    Guid Id,
    string Name,
    string? Code,
    decimal Price,
    Guid? CategoryId,
    string? CategoryName,
    bool IsActive);
