using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyOrders;

public sealed class GetRadiologyOrdersQueryHandler(RadiologyDbContext db)
    : IRequestHandler<GetRadiologyOrdersQuery, Result<PagedRadiologyOrdersDto>>
{
    public async Task<Result<PagedRadiologyOrdersDto>> Handle(GetRadiologyOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.RadiologyOrders
            .Include(o => o.RadiologyType)
            .AsQueryable();

        if (request.PatientId.HasValue) query = query.Where(o => o.PatientId == request.PatientId);
        if (request.DoctorId.HasValue) query = query.Where(o => o.DoctorId == request.DoctorId);
        if (!string.IsNullOrEmpty(request.Status)) query = query.Where(o => o.Status == request.Status);
        if (request.From.HasValue) query = query.Where(o => o.OrderDate >= request.From.Value);
        if (request.To.HasValue) query = query.Where(o => o.OrderDate <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new RadiologyOrderSummaryDto(
                o.Id, o.OrderNumber, o.Status, o.RadiologyType.Name,
                o.Price, o.IsExternalPatient, o.ExternalPatientName,
                o.PatientId, o.DoctorId, o.OrderDate))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedRadiologyOrdersDto(items, total, request.Page, request.PageSize));
    }
}
