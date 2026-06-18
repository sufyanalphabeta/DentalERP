using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Assignments.AssignDoctor;

public sealed class AssignDoctorCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<AssignDoctorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AssignDoctorCommand request, CancellationToken ct)
    {
        var hasActive = await db.DoctorAssignments
            .AnyAsync(a => a.PatientId == request.PatientId
                        && a.DoctorId == request.DoctorId
                        && a.Status == "Active", ct);

        if (hasActive)
            return Result.Failure<Guid>(new Error("Assignment.AlreadyActive",
                "This doctor already has an active assignment for this patient."));

        var assignment = DoctorAssignment.Create(
            request.PatientId,
            request.DoctorId,
            request.IsPrimary,
            request.Notes,
            request.AssignedById);

        db.DoctorAssignments.Add(assignment);
        await db.SaveChangesAsync(ct);

        await timeline.RecordAsync(
            request.PatientId,
            PatientTimelineEvent.EventTypes.DoctorAssigned,
            "تعيين طبيب جديد",
            PatientTimelineEvent.Categories.Administrative,
            actorId: request.AssignedById,
            linkedEntityType: "DoctorAssignment",
            linkedEntityId: assignment.Id,
            ct: ct);

        return Result.Success(assignment.Id);
    }
}
