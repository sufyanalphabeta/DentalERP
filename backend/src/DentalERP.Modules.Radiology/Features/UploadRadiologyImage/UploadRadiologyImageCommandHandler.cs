using DentalERP.Modules.Radiology.Domain.Entities;
using DentalERP.Modules.Radiology.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Radiology.Features.UploadRadiologyImage;

public sealed class UploadRadiologyImageCommandHandler(RadiologyDbContext db)
    : IRequestHandler<UploadRadiologyImageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadRadiologyImageCommand request, CancellationToken cancellationToken)
    {
        var order = await db.RadiologyOrders
            .Include(o => o.Images)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<Guid>(new Error("RadiologyOrder.NotFound", "Radiology order not found."));

        if (order.Status == "Cancelled")
            return Result.Failure<Guid>(new Error("RadiologyOrder.Cancelled", "Cannot upload image to a cancelled order."));

        var image = RadiologyImage.Create(
            request.OrderId, request.StorageBucket, request.StorageKey,
            request.FileName, request.FileSize, request.ContentType, request.UploadedById);

        order.AddImage(image);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(image.Id);
    }
}
