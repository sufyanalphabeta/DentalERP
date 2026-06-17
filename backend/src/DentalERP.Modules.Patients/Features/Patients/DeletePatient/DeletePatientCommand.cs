using DentalERP.SharedKernel.Behaviors;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Patients.Features.Patients.DeletePatient;

[RequirePermission("Patients.Delete")]
public sealed record DeletePatientCommand(Guid Id) : IRequest<Result>;
