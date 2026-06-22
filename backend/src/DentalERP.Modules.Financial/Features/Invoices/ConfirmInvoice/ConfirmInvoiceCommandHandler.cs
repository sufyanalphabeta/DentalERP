using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Financial.Features.Invoices.ConfirmInvoice;

public sealed class ConfirmInvoiceCommandHandler(FinancialDbContext db)
    : IRequestHandler<ConfirmInvoiceCommand, Result>
{
    public async Task<Result> Handle(ConfirmInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
            return Result.Failure(new Error("Invoice.NotFound", "الفاتورة غير موجودة"));

        var result = invoice.Confirm();
        if (!result.IsSuccess) return result;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
