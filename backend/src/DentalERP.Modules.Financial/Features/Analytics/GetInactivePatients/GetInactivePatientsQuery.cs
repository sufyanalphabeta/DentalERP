using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Analytics.GetInactivePatients;

public sealed record GetInactivePatientsQuery(int MonthsInactive = 6) : IRequest<Result<List<InactivePatientDto>>>;

public sealed record InactivePatientDto(
    Guid PatientId,
    string PatientName,
    string? Phone,
    DateTime? LastVisit,
    int MonthsSinceLastVisit
);
