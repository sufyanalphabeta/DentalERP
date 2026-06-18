using DentalERP.Modules.Financial.Domain.Entities;
using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.Modules.Financial.Services;
using DentalERP.SharedKernel.Results;
using MediatR;

namespace DentalERP.Modules.Financial.Features.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(
    FinancialDbContext db,
    IInvoiceNumberGenerator numberGenerator)
    : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (!request.Items.Any())
            return Result.Failure<Guid>(new Error("Invoice.NoItems", "يجب إضافة بند واحد على الأقل للفاتورة"));

        var invoiceNumber = await numberGenerator.GenerateAsync(cancellationToken);
        var invoice = Invoice.Create(
            invoiceNumber,
            request.PatientId,
            request.DoctorId,
            request.CreatedByUserId,
            request.Notes);

        foreach (var item in request.Items)
        {
            var invoiceItem = InvoiceItem.Create(
                invoice.Id,
                item.ServiceName,
                item.UnitPrice,
                item.Quantity,
                item.Discount,
                item.ProcedureId,
                item.ServiceCode);
            invoice.AddItem(invoiceItem);
        }

        var confirmResult = invoice.Confirm();
        if (!confirmResult.IsSuccess)
            return Result.Failure<Guid>(confirmResult.Error!);

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(invoice.Id);
    }
}
