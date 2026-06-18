using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.UpdateSupplier;

public sealed record UpdateSupplierCommand(
    Guid SupplierId, string Name, string? NameAr, string? Category,
    string? ContactPerson, string? Phone, string? Email, string? Address,
    int PaymentTermsDays, decimal CreditLimit, string? Notes) : IRequest<Result>;

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateSupplierCommandHandler(PurchasingDbContext db)
    : IRequestHandler<UpdateSupplierCommand, Result>
{
    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier is null) return Result.Failure(Error.NotFound("Supplier"));

        try
        {
            supplier.Update(request.Name, request.NameAr, request.Category, request.ContactPerson,
                request.Phone, request.Email, request.Address, request.PaymentTermsDays,
                request.CreditLimit, request.Notes);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation("Category", ex.Message));
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
