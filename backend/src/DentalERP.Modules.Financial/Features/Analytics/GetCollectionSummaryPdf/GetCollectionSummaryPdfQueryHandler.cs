using DentalERP.Modules.Financial.Features.Analytics.GetCollectionSummary;
using DentalERP.SharedKernel.Results;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Financial.Features.Analytics.GetCollectionSummaryPdf;

public sealed record GetCollectionSummaryPdfQuery(
    DateOnly From, DateOnly To,
    string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetCollectionSummaryPdfQueryHandler(IMediator mediator)
    : IRequestHandler<GetCollectionSummaryPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetCollectionSummaryPdfQuery request, CancellationToken ct)
    {
        var dataResult = await mediator.Send(new GetCollectionSummaryQuery(request.From, request.To), ct);
        if (!dataResult.IsSuccess) return Result.Failure<byte[]>(dataResult.Error);

        var data = dataResult.Value;

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
                    col.Item().AlignCenter().Text("تقرير التحصيلات / Collection Summary").FontSize(12).SemiBold();
                    col.Item().AlignCenter().Text($"من {request.From:dd/MM/yyyy} إلى {request.To:dd/MM/yyyy}").FontSize(9);
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Item().PaddingBottom(12).Row(r =>
                    {
                        r.ConstantItem(180).Text("إجمالي التحصيلات:").SemiBold().FontSize(12);
                        r.RelativeItem().Text($"{data.TotalCollected:N2} د.ل").Bold().FontSize(14);
                    });

                    col.Item().LineHorizontal(0.5f);

                    // By Vault
                    col.Item().PaddingTop(10).Text("حسب الخزينة").SemiBold().FontSize(11);
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(2); });
                        table.Header(h => { h.Cell().Background("#f3f4f6").Padding(4).Text("الخزينة").SemiBold(); h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("المبلغ").SemiBold(); });
                        foreach (var v in data.ByVault)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(v.VaultName);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{v.Amount:N2} د.ل");
                        }
                    });

                    // By Method
                    col.Item().PaddingTop(12).Text("حسب طريقة الدفع").SemiBold().FontSize(11);
                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(2); });
                        table.Header(h => { h.Cell().Background("#f3f4f6").Padding(4).Text("الطريقة").SemiBold(); h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("المبلغ").SemiBold(); });
                        foreach (var m in data.ByMethod)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(m.MethodAr);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{m.Amount:N2} د.ل");
                        }
                    });

                    // Daily breakdown
                    if (data.Daily.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("التفصيل اليومي").SemiBold().FontSize(11);
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); });
                            table.Header(h =>
                            {
                                h.Cell().Background("#f3f4f6").Padding(4).Text("التاريخ").SemiBold();
                                h.Cell().Background("#f3f4f6").Padding(4).AlignCenter().Text("عدد المعاملات").SemiBold();
                                h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("المبلغ").SemiBold();
                            });
                            foreach (var d in data.Daily)
                            {
                                table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(d.Date.ToString("dd/MM/yyyy"));
                                table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignCenter().Text(d.TransactionCount.ToString());
                                table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{d.Amount:N2} د.ل");
                            }
                        });
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
