using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Assignments.AssignDoctor;

public sealed record AssignDoctorCommand(
    Guid PatientId,
    Guid DoctorId,
    bool IsPrimary = false,
    string? Notes = null,
    Guid? AssignedById = null) : IRequest<Result<Guid>>;
