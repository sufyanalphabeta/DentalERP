using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.Modules.Financial.Services;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Payments.AddPayment;

public sealed class AddPaymentCommandHandler(FinancialDbContext db, ICommissionEngine commissionEngine)
    : IRequestHandler<AddPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Payment.ValidMethods.Contains(request.PaymentMethod))
            return Result.Failure<Guid>(new Error("Payment.InvalidMethod", "طريقة الدفع غير صحيحة"));

        var vault = await db.Vaults.FindAsync([request.VaultId], cancellationToken);
        if (vault is null || !vault.IsActive)
            return Result.Failure<Guid>(new Error("Vault.NotFound", "الخزينة غير موجودة أو غير نشطة"));

        var invoice = await db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result.Failure<Guid>(new Error("Invoice.NotFound", "الفاتورة غير موجودة"));

        var applyResult = invoice.ApplyPayment(request.Amount);
        if (!applyResult.IsSuccess)
            return Result.Failure<Guid>(applyResult.Error!);

        var payment = Payment.Create(
            request.InvoiceId,
            request.VaultId,
            request.Amount,
            request.PaymentMethod,
            request.ReferenceNumber,
            request.Notes,
            request.CreatedByUserId);

        var vaultTx = VaultTransaction.Create(
            request.VaultId,
            "receipt_from_patient",
            request.Amount,
            "in",
            relatedInvoiceId: request.InvoiceId,
            relatedPatientId: invoice.PatientId,
            notes: request.Notes,
            createdByUserId: request.CreatedByUserId);

        db.Payments.Add(payment);
        db.VaultTransactions.Add(vaultTx);

        // Cash-Basis Commission: calculate when invoice becomes fully Paid
        if (invoice.Status == "Paid")
        {
            var commission = await commissionEngine.CalculateAsync(
                invoice.DoctorId,
                invoice.Id,
                payment.Id,
                request.Amount,
                ct: cancellationToken);

            if (commission is not null)
                db.CommissionRecords.Add(commission);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(payment.Id);
    }
}
