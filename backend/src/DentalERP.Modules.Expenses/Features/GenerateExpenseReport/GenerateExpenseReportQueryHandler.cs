using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Abstractions;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Expenses.Features.GenerateExpenseReport;

internal sealed class GenerateExpenseReportQueryHandler
    : IRequestHandler<GenerateExpenseReportQuery, Result<byte[]>>
{
    private readonly ExpensesDbContext _db;
    public GenerateExpenseReportQueryHandler(ExpensesDbContext db) => _db = db;

    public async Task<Result<byte[]>> Handle(GenerateExpenseReportQuery request, CancellationToken ct)
    {
        var query = _db.Expenses.AsQueryable()
            .Where(x => x.ExpenseDate >= request.DateFrom && x.ExpenseDate <= request.DateTo);

        if (!string.IsNullOrWhiteSpace(request.CostCenter))
            query = query.Where(x => x.CostCenter == request.CostCenter);
        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId);

        var expenses = await query.OrderBy(x => x.ExpenseDate).ToListAsync(ct);

        var categories = await _db.ExpenseCategories.AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var totalAmount = expenses.Sum(x => x.Amount);
        var generatedAt = DateTime.UtcNow;

        QuestPDF.Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(s => s.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("EXPENSE REPORT").Bold().FontSize(16).AlignCenter();
                    col.Item().Text($"Period: {request.DateFrom:dd/MM/yyyy} - {request.DateTo:dd/MM/yyyy}")
                        .FontSize(11).AlignCenter();
                    if (!string.IsNullOrWhiteSpace(request.CostCenter))
                        col.Item().Text($"Cost Center: {request.CostCenter}").AlignCenter();
                    col.Item().Text($"Generated: {generatedAt:dd/MM/yyyy HH:mm} UTC").AlignCenter().FontSize(9);
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(90);  // Number
                            c.ConstantColumn(70);  // Date
                            c.RelativeColumn();    // Description
                            c.ConstantColumn(80);  // Category
                            c.ConstantColumn(70);  // Cost Center
                            c.ConstantColumn(80);  // Amount
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Expense #").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Date").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Description").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Category").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Cost Center").Bold();
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Amount").Bold().AlignRight();
                        });

                        foreach (var exp in expenses)
                        {
                            var catName = exp.CategoryId.HasValue && categories.ContainsKey(exp.CategoryId.Value)
                                ? categories[exp.CategoryId.Value] : "-";
                            table.Cell().Padding(4).Text(exp.ExpenseNumber);
                            table.Cell().Padding(4).Text(exp.ExpenseDate.ToString("dd/MM/yyyy"));
                            table.Cell().Padding(4).Text(exp.Description);
                            table.Cell().Padding(4).Text(catName);
                            table.Cell().Padding(4).Text(exp.CostCenter);
                            table.Cell().Padding(4).Text(exp.Amount.ToString("N2")).AlignRight();
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight()
                        .Text($"TOTAL: {totalAmount:N2}").Bold().FontSize(12);
                    col.Item().PaddingTop(4).AlignRight()
                        .Text($"Records: {expenses.Count}").FontSize(9);
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

