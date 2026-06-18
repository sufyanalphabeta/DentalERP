using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Inventory.Features.LookupItemByBarcode;

public sealed record LookupItemByBarcodeQuery(string Barcode) : IRequest<Result<BarcodeItemDto>>;

public sealed record BarcodeItemDto(Guid ItemId, string ItemCode, string Name, string? NameAr, decimal UnitCost);
