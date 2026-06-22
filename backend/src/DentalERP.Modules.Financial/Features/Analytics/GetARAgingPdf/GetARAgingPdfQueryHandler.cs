using DentalERP.Modules.Financial.Features.Analytics.GetARAgingReport;
using DentalERP.SharedKernel.Results;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Financial.Features.Analytics.GetARAgingPdf;

public sealed record GetARAgingPdfQuery(string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetARAgingPdfQueryHandler(IMediator mediator)
    : IRequestHandler<GetARAgingPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetARAgingPdfQuery request, CancellationToken ct)
    {
        var dataResult = await mediator.Send(new GetARAgingReportQuery(), ct);
        if (!dataResult.IsSuccess) return Result.Failure<byte[]>(dataResult.Error);

        var data = dataResult.Value;

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(s => s.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text(request.ClinicName).FontSize(16).Bold();
                    col.Item().AlignCenter().Text("تقرير تقادم الذمم المدينة / AR Aging Report").FontSize(11).SemiBold();
                    col.Item().AlignCenter().Text($"بتاريخ: {data.AsOf:dd/MM/yyyy}").FontSize(9);
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Summary totals
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r => { r.ConstantItem(150).Text("إجمالي الذمم المدينة:").SemiBold(); r.RelativeItem().Text($"{data.TotalOutstanding:N2} د.ل").Bold(); });
                            c.Item().PaddingTop(4).Row(r => { r.ConstantItem(150).Text("عدد المرضى:").SemiBold(); r.RelativeItem().Text(data.Patients.Count.ToString()); });
                        });
                    });

                    col.Item().LineHorizontal(0.5f);
                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            foreach (var hdr in new[] { "المريض", "الحالي (0-30)", "31-60 يوم", "61-90 يوم", "91-120 يوم", "+120 يوم", "الإجمالي" })
                                h.Cell().Background("#f3f4f6").Padding(4).Text(hdr).SemiBold();
                        });

                        foreach (var p in data.Patients)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(p.PatientName);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(p.Current > 0 ? $"{p.Current:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(p.Days30 > 0 ? $"{p.Days30:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(p.Days60 > 0 ? $"{p.Days60:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(p.Days90 > 0 ? $"{p.Days90:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text(p.Over90 > 0 ? $"{p.Over90:N2}" : "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{p.Total:N2}").Bold();
                        }

                        // Totals row
                        table.Cell().Background("#f9fafb").Padding(4).Text("الإجمالي").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.Patients.Sum(p => p.Current):N2}").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.Patients.Sum(p => p.Days30):N2}").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.Patients.Sum(p => p.Days60):N2}").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.Patients.Sum(p => p.Days90):N2}").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.Patients.Sum(p => p.Over90):N2}").Bold();
                        table.Cell().Background("#f9fafb").Padding(4).AlignRight().Text($"{data.TotalOutstanding:N2}").Bold();
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
