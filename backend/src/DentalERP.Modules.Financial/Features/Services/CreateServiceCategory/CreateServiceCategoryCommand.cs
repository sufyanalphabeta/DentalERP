using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(string Name, short SortOrder = 0) : IRequest<Result<Guid>>;
