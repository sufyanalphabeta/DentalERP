using DentalERP.Modules.Purchasing.Documents;
using QuestPDF.Fluent;
using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoiceDetail;
using DentalERP.SharedKernel.Documents;
using DentalERP.SharedKernel.Results;
using MediatR;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseInvoicePdf;

public sealed record GetPurchaseInvoicePdfQuery(
    Guid InvoiceId,
    bool Landscape = false
) : IRequest<Result<byte[]>>;

internal sealed class GetPurchaseInvoicePdfQueryHandler(IMediator mediator)
    : IRequestHandler<GetPurchaseInvoicePdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetPurchaseInvoicePdfQuery request, CancellationToken ct)
    {
        // Load invoice data and company settings in parallel
        var invoiceTask  = mediator.Send(new GetPurchaseInvoiceDetailQuery(request.InvoiceId), ct);
        var settingsTask = mediator.Send(new GetCompanySettingsQuery(), ct);

        await Task.WhenAll(invoiceTask, settingsTask);

        var invoiceResult = invoiceTask.Result;
        if (!invoiceResult.IsSuccess)
            return Result.Failure<byte[]>(invoiceResult.Error);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc   = new PurchaseInvoiceDocument(invoiceResult.Value, settingsTask.Result, request.Landscape);
        var bytes = doc.GeneratePdf();

        return Result.Success(bytes);
    }
}
