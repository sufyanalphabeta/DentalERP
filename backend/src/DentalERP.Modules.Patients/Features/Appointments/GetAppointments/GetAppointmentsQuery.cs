using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Appointments.GetAppointments;

public sealed record GetAppointmentsQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? DoctorId = null,
    Guid? PatientId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<Result<GetAppointmentsResponse>>;

public sealed record GetAppointmentsResponse(
    IReadOnlyList<AppointmentItem> Items,
    int TotalCount
);

public sealed record AppointmentItem(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string PatientPhone,
    Guid DoctorId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string Status,
    string? TypeName,
    string? TypeNameAr,
    string? TypeColor,
    string? ChiefComplaint
);
