using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoiceDetail;
using DentalERP.SharedKernel.Results;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Features.GetPurchaseInvoicePdf;

public sealed record GetPurchaseInvoicePdfQuery(Guid InvoiceId, string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetPurchaseInvoicePdfQueryHandler(IMediator mediator)
    : IRequestHandler<GetPurchaseInvoicePdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetPurchaseInvoicePdfQuery request, CancellationToken ct)
    {
        var detailResult = await mediator.Send(new GetPurchaseInvoiceDetailQuery(request.InvoiceId), ct);
        if (!detailResult.IsSuccess) return Result.Failure<byte[]>(detailResult.Error);

        var inv = detailResult.Value;

        var statusAr = inv.Status switch
        {
            "Draft" => "مسودة",
            "Posted" => "مرحّلة",
            "Cancelled" => "ملغاة",
            _ => inv.Status
        };

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(s => s.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(request.ClinicName).FontSize(18).Bold();
                    col.Item().AlignCenter().Text("فاتورة مشتريات / PURCHASE INVOICE").FontSize(11).Italic();
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(110).Text("رقم الفاتورة:").SemiBold(); r.RelativeItem().Text(inv.InvoiceNumber); });
                            c.Item().PaddingTop(4).Row(r => { r.ConstantItem(110).Text("تاريخ الفاتورة:").SemiBold(); r.RelativeItem().Text(inv.InvoiceDate.ToString("dd/MM/yyyy")); });
                            c.Item().PaddingTop(4).Row(r => { r.ConstantItem(110).Text("الحالة:").SemiBold(); r.RelativeItem().Text(statusAr); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(110).Text("المورد:").SemiBold(); r.RelativeItem().Text(inv.SupplierName); });
                            if (!string.IsNullOrWhiteSpace(inv.SupplierPhone))
                                c.Item().PaddingTop(4).Row(r => { r.ConstantItem(110).Text("هاتف المورد:").SemiBold(); r.RelativeItem().Text(inv.SupplierPhone!); });
                            if (!string.IsNullOrWhiteSpace(inv.WarehouseName))
                                c.Item().PaddingTop(4).Row(r => { r.ConstantItem(110).Text("المستودع:").SemiBold(); r.RelativeItem().Text(inv.WarehouseName!); });
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            foreach (var hdr in new[] { "الصنف", "الكمية", "الوحدة", "سعر الشراء", "الخصم", "الإجمالي" })
                                h.Cell().Background("#f3f4f6").Padding(4).Text(hdr).SemiBold();
                        });

                        foreach (var item in inv.Items)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).Text($"{item.ItemName}{(string.IsNullOrEmpty(item.ItemCode) ? "" : $" [{item.ItemCode}]")}");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignCenter().Text($"{item.Quantity:N2}");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).Text(item.UnitName ?? "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text($"{item.PurchasePrice:N2}");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text("—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text($"{item.LineTotal:N2}");
                        }
                    });

                    col.Item().PaddingTop(12).AlignRight().Column(totals =>
                    {
                        totals.Item().Row(r => { r.ConstantItem(130).AlignRight().Text("المجموع:").SemiBold(); r.ConstantItem(90).AlignRight().Text($"{inv.Subtotal:N2} د.ل"); });
                        if (inv.Discount > 0)
                            totals.Item().Row(r => { r.ConstantItem(130).AlignRight().Text("الخصم:").SemiBold(); r.ConstantItem(90).AlignRight().Text($"- {inv.Discount:N2} د.ل"); });
                        totals.Item().PaddingTop(4).Row(r => { r.ConstantItem(130).AlignRight().Text("الصافي:").FontSize(12).Bold(); r.ConstantItem(90).AlignRight().Text($"{inv.NetTotal:N2} د.ل").FontSize(12).Bold(); });
                    });

                    if (!string.IsNullOrWhiteSpace(inv.Notes))
                    {
                        col.Item().PaddingTop(14).Row(r => { r.ConstantItem(80).Text("ملاحظات:").SemiBold(); r.RelativeItem().Text(inv.Notes!); });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span($"تم الإنشاء: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).Italic();
                });
            });
        }).GeneratePdf();

        return Result.Success(pdfBytes);
    }
}
