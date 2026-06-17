using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.GetPatients;

public sealed record GetPatientsQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<GetPatientsResponse>>;

public sealed record GetPatientsResponse(
    IReadOnlyList<PatientSummary> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed record PatientSummary(
    Guid Id,
    string FileNumber,
    string FullName,
    string Phone,
    string? Gender,
    int? Age,
    bool IsActive
);
