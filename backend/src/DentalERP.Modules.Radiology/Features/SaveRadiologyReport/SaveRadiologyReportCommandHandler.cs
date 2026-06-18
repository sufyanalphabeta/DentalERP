using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.SaveRadiologyReport;

public sealed class SaveRadiologyReportCommandHandler(RadiologyDbContext db)
    : IRequestHandler<SaveRadiologyReportCommand, Result>
{
    public async Task<Result> Handle(SaveRadiologyReportCommand request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders
            .Include(o => o.Report)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        Result result;
        if (order.Report is null)
        {
            result = order.SaveReport(request.ReportText, request.ReportedById);
        }
        else
        {
            result = order.UpdateReport(request.ReportText);
        }

        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
