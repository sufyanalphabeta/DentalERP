using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.GetServiceCategories;

public sealed record GetServiceCategoriesQuery : IRequest<Result<List<ServiceCategoryDto>>>;

public sealed record ServiceCategoryDto(Guid Id, string Name, short SortOrder, bool IsActive);
