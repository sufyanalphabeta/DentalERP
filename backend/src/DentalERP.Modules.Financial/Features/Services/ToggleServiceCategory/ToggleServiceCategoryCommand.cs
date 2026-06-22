using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.ToggleServiceCategory;

public sealed record ToggleServiceCategoryCommand(Guid Id) : IRequest<Result<bool>>;
