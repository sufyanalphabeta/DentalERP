using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.CancelInvoice;

public sealed class CancelInvoiceCommandHandler(FinancialDbContext db)
    : IRequestHandler<CancelInvoiceCommand, Result>
{
    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result.Failure(new Error("Invoice.ReasonRequired", "سبب الإلغاء إلزامي"));

        var invoice = await db.Invoices.FindAsync([request.InvoiceId], cancellationToken);
        if (invoice is null)
            return Result.Failure(new Error("Invoice.NotFound", "الفاتورة غير موجودة"));

        var result = invoice.Cancel(request.Reason);
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
