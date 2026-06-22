using DentalERP.Modules.Financial.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Financial.Features.Invoices.GetInvoicePdf;

public sealed record GetInvoicePdfQuery(Guid InvoiceId, string ClinicName = "عيادة الأسنان") : IRequest<Result<byte[]>>;

internal sealed class GetInvoicePdfQueryHandler(FinancialDbContext db)
    : IRequestHandler<GetInvoicePdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetInvoicePdfQuery request, CancellationToken ct)
    {
        var invoice = await db.Invoices
            .Include(i => i.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice is null)
            return Result.Failure<byte[]>(new Error("Invoice.NotFound", "الفاتورة غير موجودة"));

        var patientName = await db.PatientNames
            .Where(p => p.Id == invoice.PatientId)
            .Select(p => p.FullName)
            .FirstOrDefaultAsync(ct) ?? "—";

        var doctorName = await db.UserNames
            .Where(u => u.Id == invoice.DoctorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct) ?? "—";

        var payments = await db.Payments
            .Where(p => p.InvoiceId == invoice.Id)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.Amount, p.PaymentMethod, p.CreatedAt })
            .ToListAsync(ct);

        var statusAr = invoice.Status switch
        {
            "Draft" => "مسودة",
            "Confirmed" => "مؤكدة",
            "PartiallyPaid" => "مدفوعة جزئياً",
            "Paid" => "مدفوعة",
            "Cancelled" => "ملغاة",
            _ => invoice.Status
        };

        var methodAr = (string m) => m switch
        {
            "cash" => "نقدي",
            "bank_transfer" => "تحويل بنكي",
            "card" => "بطاقة",
            "pos" => "POS",
            "insurance" => "تأمين",
            _ => m
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
                    col.Item().AlignCenter().Text(request.ClinicName)
                        .FontSize(18).Bold();
                    col.Item().AlignCenter().Text("فاتورة ضريبية / TAX INVOICE")
                        .FontSize(11).Italic();
                    col.Item().PaddingTop(6).LineHorizontal(1);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    // Invoice meta
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(100).Text("رقم الفاتورة:").SemiBold();
                                r.RelativeItem().Text(invoice.InvoiceNumber);
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("التاريخ:").SemiBold();
                                r.RelativeItem().Text(invoice.CreatedAt.ToString("dd/MM/yyyy"));
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("الحالة:").SemiBold();
                                r.RelativeItem().Text(statusAr);
                            });
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(100).Text("المريض:").SemiBold();
                                r.RelativeItem().Text(patientName);
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("الطبيب:").SemiBold();
                                r.RelativeItem().Text(doctorName);
                            });
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    // Items table
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#f3f4f6").Padding(4).Text("الخدمة").SemiBold();
                            h.Cell().Background("#f3f4f6").Padding(4).AlignCenter().Text("الكمية").SemiBold();
                            h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("سعر الوحدة").SemiBold();
                            h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("الخصم").SemiBold();
                            h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("الإجمالي").SemiBold();
                        });

                        foreach (var item in invoice.Items)
                        {
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).Text(item.ServiceName);
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text($"{item.UnitPrice:N2}");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text($"{item.Discount:N2}");
                            table.Cell().BorderBottom(0.3f).BorderColor("#e5e7eb").Padding(5).AlignRight().Text($"{item.Total:N2}");
                        }
                    });

                    // Totals
                    col.Item().PaddingTop(12).AlignRight().Column(totals =>
                    {
                        totals.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().Text("المجموع الفرعي:").SemiBold();
                            r.ConstantItem(90).AlignRight().Text($"{invoice.Subtotal:N2} {invoice.Currency}");
                        });
                        if (invoice.DiscountTotal > 0)
                        {
                            totals.Item().Row(r =>
                            {
                                r.ConstantItem(130).AlignRight().Text("إجمالي الخصم:").SemiBold();
                                r.ConstantItem(90).AlignRight().Text($"- {invoice.DiscountTotal:N2} {invoice.Currency}");
                            });
                        }
                        totals.Item().PaddingTop(4).Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().Text("الإجمالي:").FontSize(12).Bold();
                            r.ConstantItem(90).AlignRight().Text($"{invoice.TotalAmount:N2} {invoice.Currency}").FontSize(12).Bold();
                        });
                        totals.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().Text("المدفوع:").SemiBold();
                            r.ConstantItem(90).AlignRight().Text($"{invoice.PaidAmount:N2} {invoice.Currency}");
                        });
                        totals.Item().Row(r =>
                        {
                            r.ConstantItem(130).AlignRight().Text("المتبقي:").Bold();
                            r.ConstantItem(90).AlignRight().Text($"{invoice.Remaining:N2} {invoice.Currency}").Bold();
                        });
                    });

                    // Payments
                    if (payments.Count > 0)
                    {
                        col.Item().PaddingTop(16).LineHorizontal(0.5f);
                        col.Item().PaddingTop(8).Text("سجل المدفوعات").SemiBold().FontSize(11);
                        col.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#f3f4f6").Padding(4).Text("التاريخ").SemiBold();
                                h.Cell().Background("#f3f4f6").Padding(4).Text("طريقة الدفع").SemiBold();
                                h.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text("المبلغ").SemiBold();
                            });

                            foreach (var p in payments)
                            {
                                table.Cell().Padding(4).Text(p.CreatedAt.ToString("dd/MM/yyyy"));
                                table.Cell().Padding(4).Text(methodAr(p.PaymentMethod));
                                table.Cell().Padding(4).AlignRight().Text($"{p.Amount:N2} {invoice.Currency}");
                            }
                        });
                    }

                    // Notes
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        col.Item().PaddingTop(14).Row(r =>
                        {
                            r.ConstantItem(80).Text("ملاحظات:").SemiBold();
                            r.RelativeItem().Text(invoice.Notes!);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("شكراً لثقتكم — ").FontSize(8).Italic();
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd")).FontSize(8).Italic();
                });
            });
        }).GeneratePdf();

        return Result.Success(pdfBytes);
    }
}
