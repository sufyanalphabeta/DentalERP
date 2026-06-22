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
            query = query.Where(o => o.CreatedAt >= DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc));
        if (request.To.HasValue)
            query = query.Where(o => o.CreatedAt <= DateTime.SpecifyKind(request.To.Value.AddDays(1), DateTimeKind.Utc));

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new { o.Id, o.OrderNumber, o.PatientId, o.DoctorId, o.LabId, o.Status, o.Description, o.TotalCost, o.TotalRevenue, o.IsExternal, o.CreatedAt })
            .ToListAsync(cancellationToken);

        var patientIds = rows.Select(o => o.PatientId).Distinct().ToList();
        var doctorIds = rows.Where(o => o.DoctorId.HasValue).Select(o => o.DoctorId!.Value).Distinct().ToList();
        var labIds = rows.Where(o => o.LabId.HasValue).Select(o => o.LabId!.Value).Distinct().ToList();

        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var doctorNames = await db.UserNames
            .Where(u => doctorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var labNames = await db.ExternalLabs
            .Where(l => labIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, cancellationToken);

        var items = rows.Select(o => new LabOrderSummaryDto(
            o.Id,
            o.OrderNumber,
            o.PatientId,
            patientNames.GetValueOrDefault(o.PatientId, "—"),
            o.DoctorId,
            o.DoctorId.HasValue ? doctorNames.GetValueOrDefault(o.DoctorId.Value) : null,
            o.LabId.HasValue ? labNames.GetValueOrDefault(o.LabId.Value) : null,
            o.Status,
            o.Description,
            o.TotalCost,
            o.TotalRevenue,
            o.IsExternal,
            o.CreatedAt)).ToList();

        return Result.Success(new GetLabOrdersResponse(items, total, request.Page, request.PageSize));
    }
}
