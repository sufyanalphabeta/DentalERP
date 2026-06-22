using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Services.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(Guid Id, string Name, short SortOrder) : IRequest<Result>;
