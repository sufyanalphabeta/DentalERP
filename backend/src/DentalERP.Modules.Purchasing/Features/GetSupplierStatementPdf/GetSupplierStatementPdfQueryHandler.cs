using DentalERP.Modules.Purchasing.Features.GetSupplierStatement;
using DentalERP.SharedKernel.Results;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Features.GetSupplierStatementPdf;

public sealed record GetSupplierStatementPdfQuery(
    Guid SupplierId, DateTime? From, DateTime? To,
    string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetSupplierStatementPdfQueryHandler(IMediator mediator)
    : IRequestHandler<GetSupplierStatementPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetSupplierStatementPdfQuery request, CancellationToken ct)
    {
        var stmtResult = await mediator.Send(new GetSupplierStatementQuery(request.SupplierId, request.From, request.To), ct);
        if (!stmtResult.IsSuccess) return Result.Failure<byte[]>(stmtResult.Error);

        var stmt = stmtResult.Value;

        var typeAr = (string t) => t switch
        {
            "PurchaseInvoice" => "فاتورة مشتريات",
            "Payment" => "دفعة",
            "Return" => "مرتجع",
            _ => t
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
                    col.Item().AlignCenter().Text($"كشف حساب مورد — {stmt.SupplierName}").FontSize(12).SemiBold();
                    if (request.From.HasValue || request.To.HasValue)
                    {
                        var period = $"{(request.From.HasValue ? request.From.Value.ToString("dd/MM/yyyy") : "—")} إلى {(request.To.HasValue ? request.To.Value.ToString("dd/MM/yyyy") : "—")}";
                        col.Item().AlignCenter().Text(period).FontSize(9);
                    }
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    // Summary row
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(130).Text("الرصيد الافتتاحي:").SemiBold(); r.RelativeItem().Text($"{stmt.OpeningBalance:N2} د.ل"); });
                            c.Item().PaddingTop(4).Row(r => { r.ConstantItem(130).Text("إجمالي المشتريات:").SemiBold(); r.RelativeItem().Text($"{stmt.TotalPurchases:N2} د.ل"); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(130).Text("إجمالي المدفوعات:").SemiBold(); r.RelativeItem().Text($"{stmt.TotalPayments:N2} د.ل"); });
                            c.Item().PaddingTop(4).Row(r => { r.ConstantItem(130).Text("إجمالي المرتجعات:").SemiBold(); r.RelativeItem().Text($"{stmt.TotalReturns:N2} د.ل"); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(130).Text("الرصيد الختامي:").Bold().FontSize(11); r.RelativeItem().Text($"{stmt.ClosingBalance:N2} د.ل").Bold().FontSize(11); });
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    // Lines table
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            foreach (var hdr in new[] { "التاريخ", "النوع", "المرجع", "مدين", "دائن", "الرصيد" })
                                h.Cell().Background("#f3f4f6").Padding(4).Text(hdr).SemiBold();
                        });

                        foreach (var line in stmt.Lines)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(line.Date.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(typeAr(line.Type));
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(line.Reference);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(line.Debit > 0 ? $"{line.Debit:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(line.Credit > 0 ? $"{line.Credit:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{line.RunningBalance:N2}");
                        }
                    });
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
