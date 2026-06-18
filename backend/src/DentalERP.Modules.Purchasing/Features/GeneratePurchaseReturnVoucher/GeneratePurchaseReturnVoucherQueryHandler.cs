using DentalERP.Modules.Purchasing.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Features.GeneratePurchaseReturnVoucher;

public sealed record GeneratePurchaseReturnVoucherQuery(Guid ReturnId) : IRequest<Result<byte[]>>;

public sealed class GeneratePurchaseReturnVoucherQueryHandler(PurchasingDbContext db)
    : IRequestHandler<GeneratePurchaseReturnVoucherQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(
        GeneratePurchaseReturnVoucherQuery request, CancellationToken cancellationToken)
    {
        var ret = await db.PurchaseReturns
            .AsNoTracking()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, cancellationToken);

        if (ret is null) return Result.Failure<byte[]>(Error.NotFound("PurchaseReturn"));

        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == ret.SupplierId, cancellationToken);

        var itemIds = ret.Items.Select(i => i.ItemId).Distinct().ToList();
        var items = await db.Items.IgnoreQueryFilters()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Name, i.ItemCode })
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("PURCHASE RETURN VOUCHER")
                        .FontSize(18).Bold().AlignCenter();
                    col.Item().PaddingBottom(4).Text($"Return No: {ret.ReturnNumber}")
                        .FontSize(12).AlignCenter();
                    col.Item().LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Header info
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Supplier: {supplier?.Name ?? "?"}").SemiBold();
                            c.Item().Text($"Return Date: {ret.ReturnDate:dd/MM/yyyy}");
                            c.Item().Text($"Status: {ret.Status}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Reason: {ret.Reason}");
                            if (ret.PoId.HasValue)
                                c.Item().Text($"Linked PO: {ret.PoId}");
                            if (!string.IsNullOrWhiteSpace(ret.Notes))
                                c.Item().Text($"Notes: {ret.Notes}");
                        });
                    });

                    col.Item().PaddingTop(12).Text("Return Items").FontSize(12).Bold();
                    col.Item().LineHorizontal(0.5f);
                    col.Item().PaddingTop(4);

                    // Items table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background("#e5e7eb").Padding(4).Text("#").Bold();
                            header.Cell().Background("#e5e7eb").Padding(4).Text("Item").Bold();
                            header.Cell().Background("#e5e7eb").Padding(4).Text("Code").Bold();
                            header.Cell().Background("#e5e7eb").Padding(4).Text("Qty").Bold().AlignRight();
                            header.Cell().Background("#e5e7eb").Padding(4).Text("Unit Cost").Bold().AlignRight();
                            header.Cell().Background("#e5e7eb").Padding(4).Text("Total").Bold().AlignRight();
                        });

                        var idx = 1;
                        foreach (var item in ret.Items)
                        {
                            var bg = idx % 2 == 0 ? "#f9fafb" : "#ffffff";
                            var info = items.GetValueOrDefault(item.ItemId);
                            table.Cell().Background(bg).Padding(4).Text(idx.ToString());
                            table.Cell().Background(bg).Padding(4).Text(info?.Name ?? "?");
                            table.Cell().Background(bg).Padding(4).Text(info?.ItemCode ?? "?");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"{item.Quantity:F3}");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"{item.UnitCost:F2}");
                            table.Cell().Background(bg).Padding(4).AlignRight().Text($"{item.TotalCost:F2}");
                            idx++;
                        }
                    });

                    col.Item().PaddingTop(8).AlignRight()
                        .Text($"TOTAL: {ret.TotalAmount:N2}").FontSize(12).Bold();
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated: ").Italic();
                        x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC").Italic();
                        x.Span("   |   Page ").Italic();
                        x.CurrentPageNumber().Italic();
                        x.Span(" of ").Italic();
                        x.TotalPages().Italic();
                    });
            });
        }).GeneratePdf();

        return Result.Success(pdfBytes);
    }
}
