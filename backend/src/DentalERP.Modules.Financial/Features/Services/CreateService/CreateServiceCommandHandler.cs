using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Services.CreateService;

public sealed class CreateServiceCommandHandler(FinancialDbContext db)
    : IRequestHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        if (request.Price < 0)
            return Result.Failure<Guid>(new Error("Service.InvalidPrice", "السعر لا يمكن أن يكون سالباً"));

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await db.MedicalServices
                .AnyAsync(s => s.Code == request.Code, cancellationToken);
            if (codeExists)
                return Result.Failure<Guid>(new Error("Service.DuplicateCode", "رمز الخدمة مستخدم بالفعل"));
        }

        var service = MedicalService.Create(request.Name, request.Price, request.CategoryId, request.Code);
        db.MedicalServices.Add(service);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(service.Id);
    }
}
