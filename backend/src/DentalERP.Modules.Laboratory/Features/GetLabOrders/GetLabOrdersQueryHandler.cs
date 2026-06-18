using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Features.GetLabOrders;

public sealed class GetLabOrdersQueryHandler(LaboratoryDbContext db)
    : IRequestHandler<GetLabOrdersQuery, Result<GetLabOrdersResponse>>
{
    public async Task<Result<GetLabOrdersResponse>> Handle(GetLabOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.LabOrders.AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(o => o.PatientId == request.PatientId);
        if (request.DoctorId.HasValue)
            query = query.Where(o => o.DoctorId == request.DoctorId);
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(o => o.Status == request.Status);
        if (request.From.HasValue)
            query = query.Where(o => o.CreatedAt >= request.From);
        if (request.To.HasValue)
            query = query.Where(o => o.CreatedAt <= request.To);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new LabOrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.PatientId,
                o.DoctorId,
                db.ExternalLabs.Where(l => l.Id == o.LabId).Select(l => l.Name).FirstOrDefault(),
                o.Status,
                o.TotalCost,
                o.IsExternal,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetLabOrdersResponse(items, total, request.Page, request.PageSize));
    }
}
