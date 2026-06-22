using DentalERP.Modules.Laboratory.Domain.Entities;
using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.Modules.Laboratory.Services;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Laboratory.Features.CreateLabOrder;

public sealed class CreateLabOrderCommandHandler(LaboratoryDbContext db, ILabOrderNumberGenerator numberGen)
    : IRequestHandler<CreateLabOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLabOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = await numberGen.GenerateAsync(cancellationToken);

        var order = LabOrder.Create(
            orderNumber,
            request.PatientId,
            request.DoctorId,
            request.LabId,
            request.ClientId,
            request.ProcedureId,
            request.Description,
            request.ExpectedAt,
            request.Notes,
            request.CreatedByUserId);

        foreach (var item in request.Items)
            order.AddItem(LabOrderItem.Create(order.Id, item.ItemName, item.UnitCost, item.Quantity, item.Description));

        if (request.Revenue.HasValue)
            order.SetRevenue(request.Revenue.Value);

        db.LabOrders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(order.Id);
    }
}
