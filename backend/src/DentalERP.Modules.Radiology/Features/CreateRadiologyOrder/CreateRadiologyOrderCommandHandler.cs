using DentalERP.Modules.Radiology.Domain.Entities;
using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.Modules.Radiology.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.CreateRadiologyOrder;

public sealed class CreateRadiologyOrderCommandHandler(RadiologyDbContext db, IRadiologyOrderNumberGenerator numberGen)
    : IRequestHandler<CreateRadiologyOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRadiologyOrderCommand request, CancellationToken cancellationToken)
    {
        var typeExists = await db.RadiologyTypes.AnyAsync(t => t.Id == request.RadiologyTypeId && t.IsActive, cancellationToken);
        if (!typeExists)
            return Result.Failure<Guid>(new Error("RadiologyType.NotFound", "Radiology type not found or inactive."));

        var orderNumber = await numberGen.GenerateAsync(cancellationToken);

        RadiologyOrder order;
        try
        {
            order = RadiologyOrder.Create(
                orderNumber, request.RadiologyTypeId, request.Price,
                request.PatientId, request.IsExternalPatient,
                request.ExternalPatientName, request.ExternalPatientPhone,
                request.DoctorId, request.TechnicianId, request.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(new Error("RadiologyOrder.InvalidInput", ex.Message));
        }

        db.RadiologyOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(order.Id);
    }
}
