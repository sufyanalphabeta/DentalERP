using DentalERP.Modules.Patients.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Patients.Features.Patients.GetPatients;

public sealed class GetPatientsQueryHandler(PatientsDbContext db)
    : IRequestHandler<GetPatientsQuery, Result<GetPatientsResponse>>
{
    public async Task<Result<GetPatientsResponse>> Handle(GetPatientsQuery request, CancellationToken ct)
    {
        var query = db.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(search) ||
                p.FileNumber.Contains(search) ||
                p.Phone.Contains(search) ||
                (p.NationalId != null && p.NationalId.Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var patients = await query
            .OrderBy(p => p.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientSummary(
                p.Id,
                p.FileNumber,
                p.FullName,
                p.Phone,
                p.Gender,
                p.DateOfBirth.HasValue
                    ? (int)((DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - p.DateOfBirth.Value.DayNumber) / 365.25)
                    : null,
                p.IsActive
            ))
            .ToListAsync(ct);

        return Result.Success(new GetPatientsResponse(
            patients, totalCount, request.Page, request.PageSize, totalPages));
    }
}
