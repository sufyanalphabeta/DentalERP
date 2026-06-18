using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Domain.Internal;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace DentalERP.Modules.Purchasing.Features.RecordSupplierPayment;

public sealed record RecordSupplierPaymentCommand(
    Guid SupplierId, Guid VaultId, decimal Amount,
    DateOnly PaymentDate, string? ReferenceNumber, string? Notes,
    Guid? PaidById) : IRequest<Result<Guid>>;

public sealed class RecordSupplierPaymentCommandValidator : AbstractValidator<RecordSupplierPaymentCommand>
{
    public RecordSupplierPaymentCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.VaultId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public sealed class RecordSupplierPaymentCommandHandler(
    PurchasingDbContext db, ISupplierPaymentNumberGenerator numGen)
    : IRequestHandler<RecordSupplierPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordSupplierPaymentCommand request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier is null) return Result.Failure<Guid>(Error.NotFound("Supplier"));

        var paymentNumber = await numGen.GenerateAsync(cancellationToken);

        var payment = SupplierPayment.Create(
            paymentNumber, request.SupplierId, request.VaultId,
            request.Amount, request.PaymentDate,
            request.ReferenceNumber, request.Notes, request.PaidById);

        // Write vault transaction atomically (deduct from vault)
        var vaultTx = new VaultTransactionEntry
        {
            Id = Guid.NewGuid(),
            VaultId = request.VaultId,
            TransactionType = "supplier_payment",
            Amount = request.Amount,
            Direction = "out",
            Notes = $"Supplier payment to {supplier.Name} — {paymentNumber}",
            CreatedByUserId = request.PaidById,
            CreatedAt = DateTime.UtcNow
        };

        db.SupplierPayments.Add(payment);
        db.VaultTransactions.Add(vaultTx);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(payment.Id);
    }
}
