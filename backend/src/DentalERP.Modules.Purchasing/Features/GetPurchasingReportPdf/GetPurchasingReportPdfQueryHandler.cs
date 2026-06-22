using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Features.GetPurchasingReportPdf;

public sealed record GetPurchasingReportPdfQuery(
    Guid? SupplierId, string? Status,
    DateOnly? From, DateOnly? To,
    string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetPurchasingReportPdfQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GetPurchasingReportPdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetPurchasingReportPdfQuery request, CancellationToken ct)
    {
        var query = db.PurchaseInvoices.AsNoTracking().Where(x => x.DeletedAt == null);

        if (request.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (request.From.HasValue)
            query = query.Where(x => x.InvoiceDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(x => x.InvoiceDate <= request.To.Value);

        var invoices = await query
            .OrderByDescending(x => x.InvoiceDate)
            .ToListAsync(ct);

        var supplierIds = invoices.Select(i => i.SupplierId).Distinct().ToList();
        var suppliers = await db.Suppliers.IgnoreQueryFilters()
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var statusAr = (string s) => s switch
        {
            "Draft" => "مسودة",
            "Posted" => "مرحّلة",
            "Cancelled" => "ملغاة",
            _ => s
        };

        var total = invoices.Sum(i => i.NetTotal);

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
                    col.Item().AlignCenter().Text("تقرير فواتير المشتريات").FontSize(12).SemiBold();
                    var period = request.From.HasValue || request.To.HasValue
                        ? $"من {(request.From.HasValue ? request.From.Value.ToString("dd/MM/yyyy") : "—")} إلى {(request.To.HasValue ? request.To.Value.ToString("dd/MM/yyyy") : "—")}"
                        : "كل الفترات";
                    col.Item().AlignCenter().Text(period).FontSize(9);
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            foreach (var hdr in new[] { "رقم الفاتورة", "التاريخ", "المورد", "الحالة", "الصافي" })
                                h.Cell().Background("#f3f4f6").Padding(5).Text(hdr).SemiBold();
                        });

                        foreach (var inv in invoices)
                        {
                            var supplierName = suppliers.GetValueOrDefault(inv.SupplierId, "—");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(inv.InvoiceNumber);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(inv.InvoiceDate.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(supplierName);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).Text(statusAr(inv.Status));
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(4).AlignRight().Text($"{inv.NetTotal:N2}");
                        }
                    });

                    col.Item().PaddingTop(12).AlignRight().Row(r =>
                    {
                        r.ConstantItem(120).AlignRight().Text($"الإجمالي ({invoices.Count} فاتورة):").SemiBold();
                        r.ConstantItem(100).AlignRight().Text($"{total:N2} د.ل").Bold();
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
