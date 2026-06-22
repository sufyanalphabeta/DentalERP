using DentalERP.Modules.Purchasing.Domain.Entities;
using DentalERP.Modules.Purchasing.Domain.Internal;
using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.Modules.Purchasing.Services;
using DentalERP.SharedKernel.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        // Validate vault existence and active status
        var vaultStatus = await db.Database
            .SqlQuery<int?>($"SELECT CASE WHEN is_active THEN 1 ELSE 0 END AS \"Value\" FROM vaults WHERE id = {request.VaultId}")
            .FirstOrDefaultAsync(cancellationToken);

        if (vaultStatus is null)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "الخزينة المحددة غير موجودة."));
        if (vaultStatus == 0)
            return Result.Failure<Guid>(new Error("Vault.Inactive", "الخزينة المحددة غير نشطة."));

        var paymentNumber = await numGen.GenerateAsync(cancellationToken);

        var payment = SupplierPayment.Create(
            paymentNumber, request.SupplierId, request.VaultId,
            request.Amount, request.PaymentDate,
            request.ReferenceNumber, request.Notes, request.PaidById);

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
