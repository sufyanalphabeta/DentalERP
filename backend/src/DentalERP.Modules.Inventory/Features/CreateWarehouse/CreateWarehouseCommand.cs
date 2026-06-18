using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string Name,
    string? NameAr,
    string? Location,
    bool IsDefault) : IRequest<Result<Guid>>;
