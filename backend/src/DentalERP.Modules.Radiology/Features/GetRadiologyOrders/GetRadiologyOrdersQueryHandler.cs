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
        if (request.TypeId.HasValue) query = query.Where(o => o.RadiologyTypeId == request.TypeId);
        if (request.From.HasValue) query = query.Where(o => o.OrderDate >= DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc));
        if (request.To.HasValue) query = query.Where(o => o.OrderDate <= DateTime.SpecifyKind(request.To.Value.AddDays(1), DateTimeKind.Utc));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new { o.Id, o.OrderNumber, o.Status, TypeName = o.RadiologyType.Name, o.Price, o.IsExternalPatient, o.ExternalPatientName, o.PatientId, o.DoctorId, o.OrderDate })
            .ToListAsync(cancellationToken);

        var patientIds = rows.Where(o => o.PatientId.HasValue).Select(o => o.PatientId!.Value).Distinct().ToList();
        var doctorIds = rows.Where(o => o.DoctorId.HasValue).Select(o => o.DoctorId!.Value).Distinct().ToList();

        var patientNames = await db.PatientNames
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.FullName, cancellationToken);

        var doctorNames = await db.UserNames
            .Where(u => doctorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var items = rows.Select(o => new RadiologyOrderSummaryDto(
            o.Id, o.OrderNumber, o.Status, o.TypeName,
            o.Price, o.IsExternalPatient, o.ExternalPatientName,
            o.PatientId,
            o.IsExternalPatient ? (o.ExternalPatientName ?? "مريض خارجي") : (o.PatientId.HasValue ? patientNames.GetValueOrDefault(o.PatientId.Value, "—") : "—"),
            o.DoctorId,
            o.DoctorId.HasValue ? doctorNames.GetValueOrDefault(o.DoctorId.Value) : null,
            o.OrderDate)).ToList();

        return Result.Success(new PagedRadiologyOrdersDto(items, total, request.Page, request.PageSize));
    }
}
