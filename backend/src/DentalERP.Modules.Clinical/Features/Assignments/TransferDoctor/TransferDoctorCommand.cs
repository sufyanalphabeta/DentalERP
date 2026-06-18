using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Clinical.Features.Assignments.TransferDoctor;

public sealed record TransferDoctorCommand(
    Guid AssignmentId,
    Guid NewDoctorId,
    string? Reason = null,
    Guid? TransferredById = null) : IRequest<Result<Guid>>;
