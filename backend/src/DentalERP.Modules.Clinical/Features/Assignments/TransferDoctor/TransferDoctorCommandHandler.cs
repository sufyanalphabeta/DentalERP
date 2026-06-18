using DentalERP.Modules.Clinical.Domain.Entities;
using DentalERP.Modules.Clinical.Infrastructure;
using DentalERP.Modules.Clinical.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Clinical.Features.Assignments.TransferDoctor;

public sealed class TransferDoctorCommandHandler(ClinicalDbContext db, ITimelineService timeline)
    : IRequestHandler<TransferDoctorCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(TransferDoctorCommand request, CancellationToken ct)
    {
        var assignment = await db.DoctorAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, ct);

        if (assignment is null)
            return Result.Failure<Guid>(new Error("Assignment.NotFound", "Assignment not found."));

        DoctorAssignment newAssignment;
        try
        {
            newAssignment = assignment.Transfer(request.NewDoctorId, request.Reason, request.TransferredById);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<Guid>(new Error("Assignment.InvalidTransfer", ex.Message));
        }

        db.DoctorAssignments.Add(newAssignment);
        await db.SaveChangesAsync(ct);

        await timeline.RecordAsync(
            assignment.PatientId,
            PatientTimelineEvent.EventTypes.DoctorTransferred,
            "تحويل المريض إلى طبيب آخر",
            PatientTimelineEvent.Categories.Administrative,
            description: request.Reason,
            actorId: request.TransferredById,
            linkedEntityType: "DoctorAssignment",
            linkedEntityId: newAssignment.Id,
            ct: ct);

        return Result.Success(newAssignment.Id);
    }
}
