using DentalERP.Modules.Assets.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Assets.Features.GenerateAssetRegister;

internal sealed class GenerateAssetRegisterQueryHandler : IRequestHandler<GenerateAssetRegisterQuery, Result<byte[]>>
{
    private readonly AssetsDbContext _db;
    public GenerateAssetRegisterQueryHandler(AssetsDbContext db) => _db = db;

    public async Task<Result<byte[]>> Handle(GenerateAssetRegisterQuery request, CancellationToken ct)
    {
        var query = _db.Assets.AsQueryable();
        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);

        var assets = await query.OrderBy(x => x.AssetTag).ToListAsync(ct);
        var cats = await _db.AssetCategories.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, ct);
        var totalCost = assets.Sum(x => x.PurchaseCost ?? 0m);

        QuestPDF.Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(s => s.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("ASSET REGISTER REPORT").Bold().FontSize(14).AlignCenter();
                    if (!string.IsNullOrWhiteSpace(request.Status))
                        col.Item().Text($"Status Filter: {request.Status}").AlignCenter();
                    col.Item().Text($"Generated: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                        .FontSize(8).AlignCenter();
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(8).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(80);   // Tag
                            c.RelativeColumn(2);    // Name
                            c.ConstantColumn(90);   // Category
                            c.ConstantColumn(70);   // Purchase Date
                            c.ConstantColumn(80);   // Cost
                            c.ConstantColumn(90);   // Location
                            c.ConstantColumn(80);   // Status
                        });

                        table.Header(h =>
                        {
                            foreach (var title in new[] { "Asset Tag", "Name", "Category", "Purchase Date", "Cost", "Location", "Status" })
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(title).Bold();
                        });

                        foreach (var a in assets)
                        {
                            var catName = a.CategoryId.HasValue && cats.ContainsKey(a.CategoryId.Value)
                                ? cats[a.CategoryId.Value] : "-";
                            table.Cell().Padding(3).Text(a.AssetTag);
                            table.Cell().Padding(3).Text(a.Name);
                            table.Cell().Padding(3).Text(catName);
                            table.Cell().Padding(3).Text(a.PurchaseDate?.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().Padding(3).Text(a.PurchaseCost.HasValue ? a.PurchaseCost.Value.ToString("N2") : "-").AlignRight();
                            table.Cell().Padding(3).Text(a.Location ?? "-");
                            table.Cell().Padding(3).Text(a.Status);
                        }
                    });

                    col.Item().PaddingTop(8).AlignRight()
                        .Text($"Total Assets: {assets.Count}   |   Total Cost: {totalCost:N2}")
                        .Bold().FontSize(10);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();

        return Result.Success(bytes);
    }
}
