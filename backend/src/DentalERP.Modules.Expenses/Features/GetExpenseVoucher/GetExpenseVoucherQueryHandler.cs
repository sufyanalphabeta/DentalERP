using DentalERP.Modules.Expenses.Infrastructure;
using DentalERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Expenses.Features.GetExpenseVoucher;

public sealed record GetExpenseVoucherQuery(Guid ExpenseId) : IRequest<Result<byte[]>>;

internal sealed class GetExpenseVoucherQueryHandler(ExpensesDbContext db)
    : IRequestHandler<GetExpenseVoucherQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(GetExpenseVoucherQuery request, CancellationToken ct)
    {
        var expense = await db.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExpenseId, ct);

        if (expense is null) return Result.Failure<byte[]>(Error.NotFound("Expense"));

        string? categoryName = null;
        if (expense.CategoryId.HasValue)
        {
            var cat = await db.ExpenseCategories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == expense.CategoryId.Value, ct);
            categoryName = cat?.Name;
        }

        string? vaultName = null;
        if (expense.VaultId.HasValue)
        {
            var vaultId = expense.VaultId.Value;
            var result = await db.Database
                .SqlQuery<string>($"SELECT name AS \"Value\" FROM vaults WHERE id = {vaultId}")
                .FirstOrDefaultAsync(ct);
            vaultName = result;
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(28, QuestPDF.Infrastructure.Unit.Point);
                page.DefaultTextStyle(s => s.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("سند صرف مصروف")
                        .FontSize(16).Bold().AlignCenter();
                    col.Item().Text("EXPENSE PAYMENT VOUCHER")
                        .FontSize(12).AlignCenter();
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        // Left column
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(90).Text("Voucher No:").SemiBold();
                                r.RelativeItem().Text(expense.ExpenseNumber);
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(90).Text("Date:").SemiBold();
                                r.RelativeItem().Text(expense.ExpenseDate.ToString("dd/MM/yyyy"));
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(90).Text("Amount:").SemiBold();
                                r.RelativeItem().Text($"{expense.Amount:N2}").FontSize(12).Bold();
                            });
                        });

                        // Right column
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(90).Text("Category:").SemiBold();
                                r.RelativeItem().Text(categoryName ?? "—");
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(90).Text("Cost Center:").SemiBold();
                                r.RelativeItem().Text(expense.CostCenter);
                            });
                            c.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(90).Text("Vault:").SemiBold();
                                r.RelativeItem().Text(vaultName ?? "—");
                            });
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    col.Item().PaddingTop(8).Row(r =>
                    {
                        r.ConstantItem(90).Text("Description:").SemiBold();
                        r.RelativeItem().Text(expense.Description);
                    });

                    if (!string.IsNullOrWhiteSpace(expense.Notes))
                    {
                        col.Item().PaddingTop(6).Row(r =>
                        {
                            r.ConstantItem(90).Text("Notes:").SemiBold();
                            r.RelativeItem().Text(expense.Notes!);
                        });
                    }

                    // Signature area
                    col.Item().PaddingTop(24).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.5f);
                            c.Item().PaddingTop(4).AlignCenter().Text("Prepared By / المُعدّ");
                        });
                        row.ConstantItem(30);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.5f);
                            c.Item().PaddingTop(4).AlignCenter().Text("Approved By / المُعتمِد");
                        });
                        row.ConstantItem(30);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.5f);
                            c.Item().PaddingTop(4).AlignCenter().Text("Received By / المُستلِم");
                        });
                    });
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).Italic();
                    });
            });
        }).GeneratePdf();

        return Result.Success(pdfBytes);
    }
}
