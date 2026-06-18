using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.GetRadiologyOrderById;

public sealed class GetRadiologyOrderByIdQueryHandler(RadiologyDbContext db)
    : IRequestHandler<GetRadiologyOrderByIdQuery, Result<RadiologyOrderDetailDto>>
{
    public async Task<Result<RadiologyOrderDetailDto>> Handle(GetRadiologyOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders
            .Include(o => o.RadiologyType)
            .Include(o => o.Images)
            .Include(o => o.Report)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
            return Result.Failure<RadiologyOrderDetailDto>(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        var dto = new RadiologyOrderDetailDto(
            order.Id, order.OrderNumber, order.Status,
            order.RadiologyTypeId, order.RadiologyType.Name,
            order.Price, order.IsExternalPatient,
            order.ExternalPatientName, order.ExternalPatientPhone,
            order.PatientId, order.DoctorId, order.TechnicianId, order.InvoiceId,
            order.DoctorCommissionAmount, order.TechCommissionAmount,
            order.Notes, order.CancellationReason, order.OrderDate,
            order.Images.Select(i => new RadiologyImageDto(i.Id, i.FileName, i.FileSize, i.ContentType, i.UploadedAt)).ToList(),
            order.Report is null ? null : new RadiologyReportDto(order.Report.Id, order.Report.ReportText, order.Report.ReportedById, order.Report.ReportedAt, order.Report.UpdatedAt)
        );

        return Result.Success(dto);
    }
}
