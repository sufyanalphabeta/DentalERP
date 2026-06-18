using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.CreateSupplier;

public sealed record CreateSupplierCommand(
    string Name, string? NameAr, string? Category,
    string? ContactPerson, string? Phone, string? Email, string? Address,
    int PaymentTermsDays, decimal CreditLimit, string? Notes) : IRequest<Result<Guid>>;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PaymentTermsDays).GreaterThan(0);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateSupplierCommandHandler(PurchasingDbContext db, ISupplierCodeGenerator codeGen)
    : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var code = await codeGen.GenerateAsync(cancellationToken);
        var supplier = Supplier.Create(code, request.Name, request.NameAr, request.Category,
            request.ContactPerson, request.Phone, request.Email, request.Address,
            request.PaymentTermsDays, request.CreditLimit, request.Notes);

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(supplier.Id);
    }
}
