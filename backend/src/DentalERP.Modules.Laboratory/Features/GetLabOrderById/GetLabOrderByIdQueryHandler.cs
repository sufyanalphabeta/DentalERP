using DentalERP.Modules.Laboratory.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Laboratory.Features.GetLabOrderById;

public sealed class GetLabOrderByIdQueryHandler(LaboratoryDbContext db)
    : IRequestHandler<GetLabOrderByIdQuery, Result<LabOrderDetailDto>>
{
    public async Task<Result<LabOrderDetailDto>> Handle(GetLabOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await db.LabOrders
            .Include(o => o.Items)
            .Include(o => o.Results)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
            return Result.Failure<LabOrderDetailDto>(new Error("LabOrder.NotFound", "الطلب غير موجود"));

        var labName = order.LabId.HasValue
            ? await db.ExternalLabs.Where(l => l.Id == order.LabId).Select(l => l.Name).FirstOrDefaultAsync(cancellationToken)
            : null;
        var clientName = order.ClientId.HasValue
            ? await db.LabClients.Where(c => c.Id == order.ClientId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken)
            : null;

        var dto = new LabOrderDetailDto(
            order.Id, order.OrderNumber, order.PatientId, order.DoctorId,
            order.LabId, labName, order.ClientId, clientName, order.IsExternal,
            order.Status, order.Description, order.SentAt, order.ExpectedAt,
            order.ReceivedAt, order.TotalCost, order.TotalRevenue, order.Currency,
            order.Notes, order.CreatedAt,
            order.Items.Select(i => new LabOrderItemDto(i.Id, i.ItemName, i.Description, i.Quantity, i.UnitCost, i.TotalCost)).ToList(),
            order.Results.Select(r => new LabResultDto(r.Id, r.ResultNotes, r.FileName, r.ReceivedAt)).ToList()
        );

        return Result.Success(dto);
    }
}
