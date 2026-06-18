using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.UpdateService;

public sealed class UpdateServiceCommandHandler(FinancialDbContext db)
    : IRequestHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await db.MedicalServices.FindAsync([request.Id], cancellationToken);
        if (service is null)
            return Result.Failure(new Error("Service.NotFound", "الخدمة غير موجودة"));

        if (request.Price < 0)
            return Result.Failure(new Error("Service.InvalidPrice", "السعر لا يمكن أن يكون سالباً"));

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await db.MedicalServices
                .AnyAsync(s => s.Code == request.Code && s.Id != request.Id, cancellationToken);
            if (codeExists)
                return Result.Failure(new Error("Service.DuplicateCode", "رمز الخدمة مستخدم بالفعل"));
        }

        service.Update(request.Name, request.Price, request.CategoryId, request.Code);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
