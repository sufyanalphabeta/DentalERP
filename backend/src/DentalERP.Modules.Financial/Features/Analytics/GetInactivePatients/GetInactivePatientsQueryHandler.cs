using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Analytics.GetInactivePatients;

public sealed class GetInactivePatientsQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInactivePatientsQuery, Result<List<InactivePatientDto>>>
{
    public async Task<Result<List<InactivePatientDto>>> Handle(GetInactivePatientsQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-request.MonthsInactive);

        // Raw SQL: patients with no completed appointment after cutoff
        var results = await db.Database.SqlQuery<InactivePatientRaw>($"""
            SELECT
                p.id AS "PatientId",
                p.full_name AS "PatientName",
                p.phone AS "Phone",
                MAX(a.scheduled_at) AS "LastVisit"
            FROM patients p
            LEFT JOIN appointments a ON a.patient_id = p.id AND a.status = 'Completed'
            WHERE p.deleted_at IS NULL
            GROUP BY p.id, p.full_name, p.phone
            HAVING MAX(a.scheduled_at) < {cutoff} OR MAX(a.scheduled_at) IS NULL
            ORDER BY MAX(a.scheduled_at) ASC NULLS FIRST
            """).ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var dtos = results.Select(r => new InactivePatientDto(
            r.PatientId,
            r.PatientName,
            r.Phone,
            r.LastVisit,
            r.LastVisit.HasValue
                ? (int)((now - r.LastVisit.Value).TotalDays / 30)
                : 999
        )).ToList();

        return Result.Success(dtos);
    }

    private sealed record InactivePatientRaw(Guid PatientId, string PatientName, string? Phone, DateTime? LastVisit);
}
