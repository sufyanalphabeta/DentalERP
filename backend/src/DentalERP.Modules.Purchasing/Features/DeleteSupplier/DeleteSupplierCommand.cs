using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.DeleteSupplier;

public sealed record DeleteSupplierCommand(Guid SupplierId) : IRequest<Result<string>>;
