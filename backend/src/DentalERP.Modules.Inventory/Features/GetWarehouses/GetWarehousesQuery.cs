using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.GetWarehouses;

public sealed record GetWarehousesQuery() : IRequest<Result<IReadOnlyList<WarehouseDto>>>;

public sealed record WarehouseDto(Guid Id, string Name, string? NameAr, string? Location, bool IsDefault, bool IsActive);
