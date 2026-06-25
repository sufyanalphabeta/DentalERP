using DentalERP.Modules.Purchasing.Features.GetPurchaseInvoiceDetail;
using DentalERP.SharedKernel.Documents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalERP.Modules.Purchasing.Documents;

/// <summary>
/// Enterprise-grade purchase invoice PDF document.
/// Uses CompanySettings from system_settings for all branding — nothing hardcoded.
/// Page format and orientation are caller-supplied for print-preview flexibility.
/// </summary>
public sealed class PurchaseInvoiceDocument : IDocument
{
    // ── Design tokens ──────────────────────────────────────────────────────────
    private const string C_Primary     = "#1e3a5f";
    private const string C_PrimaryMid  = "#2c5282";
    private const string C_Accent      = "#3b82f6";
    private const string C_HeaderText  = "#ffffff";
    private const string C_TableHdrBg  = "#1e3a5f";
    private const string C_TableHdrFg  = "#ffffff";
    private const string C_RowEven     = "#f0f4f8";
    private const string C_RowOdd      = "#ffffff";
    private const string C_Border      = "#cbd5e1";
    private const string C_InfoBg      = "#f8fafc";
    private const string C_InfoBorder  = "#e2e8f0";
    private const string C_TotalsBox   = "#eff6ff";
    private const string C_GrandBg     = "#1e3a5f";
    private const string C_GrandFg     = "#ffffff";
    private const string C_TextDark    = "#0f172a";
    private const string C_TextMid     = "#334155";
    private const string C_TextLight   = "#64748b";
    private const string C_Label       = "#94a3b8";
    private const string C_SignBorder  = "#94a3b8";
    private const string C_SignBg      = "#f8fafc";
    private const string C_TermsBg     = "#fffbeb";
    private const string C_TermsBorder = "#fde68a";

    private const float  F_Base  = 9f;
    private const float  F_Sm    = 7.5f;
    private const float  F_Xs    = 6.5f;
    private const float  F_Lg    = 11f;
    private const float  F_XL    = 14f;
    private const float  F_XXL   = 18f;
    private const string ArabicFont = "Noto Sans Arabic";

    private static bool _fontsRegistered;
    private static readonly Lock _fontLock = new();

    // ── Data ───────────────────────────────────────────────────────────────────
    private readonly PIDetailDto _inv;
    private readonly CompanySettings _co;
    private readonly bool _landscape;

    public PurchaseInvoiceDocument(PIDetailDto inv, CompanySettings co, bool landscape = false)
    {
        _inv       = inv;
        _co        = co;
        _landscape = landscape;
        EnsureFonts();
    }

    // ── Font registration ──────────────────────────────────────────────────────
    // Noto fonts are installed in the Docker container (see Dockerfile: apt-get install fonts-noto-core).
    // QuestPDF resolves them automatically by family name — no manual registration needed.
    private static void EnsureFonts()
    {
        if (_fontsRegistered) return;
        lock (_fontLock)
        {
            if (_fontsRegistered) return;
            _fontsRegistered = true;
        }
    }

    // ── IDocument ──────────────────────────────────────────────────────────────
    public DocumentMetadata GetMetadata()
    {
        var m = DocumentMetadata.Default;
        m.Title   = $"فاتورة مشتريات {_inv.InvoiceNumber}";
        m.Author  = _co.NameAr;
        m.Subject = "Purchase Invoice";
        return m;
    }

    public void Compose(IDocumentContainer container)
    {
        var pageSize = _landscape ? PageSizes.A4.Landscape() : PageSizes.A4;

        container.Page(page =>
        {
            page.Size(pageSize);
            page.MarginTop(28, Unit.Point);
            page.MarginBottom(22, Unit.Point);
            page.MarginHorizontal(36, Unit.Point);
            page.DefaultTextStyle(s => s
                .FontFamily(ArabicFont, "Lato")
                .FontSize(F_Base)
                .FontColor(C_TextDark));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingTop(8).Element(ComposeBody);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ── HEADER ─────────────────────────────────────────────────────────────────
    private void ComposeHeader(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Left: Document title & invoice meta
                row.RelativeItem(3).Column(left =>
                {
                    left.Item().Text("فاتورة مشتريات").FontSize(F_Lg).Bold().FontColor(C_Primary);
                    left.Item().Text("PURCHASE INVOICE").FontSize(F_Sm).Italic().FontColor(C_TextLight);
                    left.Item().PaddingTop(6).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.ConstantColumn(85); c.RelativeColumn(); });
                        MetaRow(t, "رقم الفاتورة:", _inv.InvoiceNumber);
                        MetaRow(t, "تاريخ الإصدار:", _inv.InvoiceDate.ToString("dd/MM/yyyy"));
                        if (_inv.PostedAt.HasValue)
                            MetaRow(t, "تاريخ الترحيل:", _inv.PostedAt.Value.ToString("dd/MM/yyyy"));
                        MetaRow(t, "الحالة:", StatusAr(_inv.Status));
                    });
                });

                // Center: Logo placeholder + company name
                row.RelativeItem(4).Column(center =>
                {
                    // Logo circle or company initial
                    center.Item().AlignCenter().Element(logoEl =>
                    {
                        logoEl.Width(48).Height(48)
                            .Background(C_Primary)
                            .Border(1).BorderColor(C_Accent)
                            .AlignCenter().AlignMiddle()
                            .Text(_co.NameAr.Length > 0 ? _co.NameAr[..1] : "ع")
                                .FontSize(20).Bold().FontColor(C_HeaderText);
                    });
                    center.Item().PaddingTop(5).AlignCenter()
                        .Text(_co.NameAr).FontSize(F_XL).Bold().FontColor(C_Primary);
                    center.Item().AlignCenter()
                        .Text(_co.NameEn).FontSize(F_Sm).FontColor(C_TextLight);
                    if (!string.IsNullOrEmpty(_co.BusinessType))
                        center.Item().AlignCenter()
                            .Text(_co.BusinessType).FontSize(F_Sm).FontColor(C_TextLight);
                });

                // Right: Company contact info
                row.RelativeItem(3).AlignRight().Column(right =>
                {
                    if (!string.IsNullOrEmpty(_co.Address))
                        right.Item().AlignRight().Text(_co.Address).FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.City) || !string.IsNullOrEmpty(_co.Country))
                        right.Item().AlignRight().Text(_co.CityCountry()).FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.Phone))
                        right.Item().AlignRight().Text($"هاتف: {_co.Phone}").FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.Mobile))
                        right.Item().AlignRight().Text($"جوال: {_co.Mobile}").FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.Email))
                        right.Item().AlignRight().Text(_co.Email).FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.TaxNumber))
                        right.Item().PaddingTop(4).AlignRight()
                            .Text($"الرقم الضريبي: {_co.TaxNumber}").FontSize(F_Sm).FontColor(C_TextMid);
                    if (!string.IsNullOrEmpty(_co.LicenseNumber))
                        right.Item().AlignRight()
                            .Text($"رقم الترخيص: {_co.LicenseNumber}").FontSize(F_Sm).FontColor(C_TextMid);
                });
            });

            // Accent rule
            col.Item().PaddingTop(8).Height(3).Background(C_Primary);
            col.Item().Height(2).Background(C_Accent);
        });
    }

    // ── BODY ───────────────────────────────────────────────────────────────────
    private void ComposeBody(IContainer c)
    {
        c.Column(col =>
        {
            // ── Section 1: Supplier + Invoice info side-by-side ────────────────
            col.Item().PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Border(0.5f).BorderColor(C_InfoBorder).Background(C_InfoBg)
                    .Padding(10).Column(supplier =>
                    {
                        SectionTitle(supplier, "بيانات المورد");
                        supplier.Item().PaddingTop(4).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(90); c.RelativeColumn(); });
                            MetaRow(t, "المورد:", _inv.SupplierName);
                            if (!string.IsNullOrEmpty(_inv.SupplierPhone))
                                MetaRow(t, "الهاتف:", _inv.SupplierPhone!);
                        });
                    });

                row.ConstantItem(12);

                row.RelativeItem().Border(0.5f).BorderColor(C_InfoBorder).Background(C_InfoBg)
                    .Padding(10).Column(invInfo =>
                    {
                        SectionTitle(invInfo, "بيانات الفاتورة");
                        invInfo.Item().PaddingTop(4).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(90); c.RelativeColumn(); });
                            if (!string.IsNullOrEmpty(_inv.WarehouseName))
                                MetaRow(t, "المستودع:", _inv.WarehouseName!);
                            MetaRow(t, "تاريخ الإنشاء:", _inv.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                        });
                    });
            });

            // ── Section 2: Items Table ─────────────────────────────────────────
            col.Item().PaddingBottom(12).Element(ComposeItemsTable);

            // ── Section 3: Totals ──────────────────────────────────────────────
            col.Item().PaddingBottom(14).AlignRight().Element(ComposeTotals);

            // ── Section 4: Notes ───────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(_inv.Notes))
            {
                col.Item().PaddingBottom(14).Border(0.5f).BorderColor(C_InfoBorder)
                    .Background(C_InfoBg).Padding(10).Column(n =>
                    {
                        SectionTitle(n, "ملاحظات");
                        n.Item().PaddingTop(4).Text(_inv.Notes!).FontSize(F_Base).FontColor(C_TextMid);
                    });
            }

            // ── Section 5: Signatures ──────────────────────────────────────────
            col.Item().PaddingBottom(14).Element(ComposeSignatures);

            // ── Section 6: Terms & Conditions ─────────────────────────────────
            if (!string.IsNullOrWhiteSpace(_co.TermsAndConditions))
                col.Item().Element(ComposeTerms);
        });
    }

    // ── ITEMS TABLE ────────────────────────────────────────────────────────────
    private void ComposeItemsTable(IContainer c)
    {
        c.Table(table =>
        {
            // Columns: # | Code | Name | Unit | Expiry | Qty | Price | Discount | Total
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(20);   // #
                cols.ConstantColumn(52);   // Code
                cols.RelativeColumn(3);    // Name
                cols.ConstantColumn(36);   // Unit
                cols.ConstantColumn(52);   // Expiry
                cols.ConstantColumn(36);   // Qty
                cols.ConstantColumn(54);   // Price
                cols.ConstantColumn(44);   // Discount
                cols.ConstantColumn(58);   // Total
            });

            // Header row
            table.Header(h =>
            {
                void Hdr(string label, bool center = true)
                {
                    var cell = h.Cell().Background(C_TableHdrBg).Padding(5);
                    var t = cell.Text(label).FontSize(F_Sm).Bold().FontColor(C_TableHdrFg);
                    if (center) t.AlignCenter();
                }

                Hdr("#");
                Hdr("كود الصنف");
                Hdr("اسم الصنف", false);
                Hdr("الوحدة");
                Hdr("الانتهاء");
                Hdr("الكمية");
                Hdr("السعر");
                Hdr("الخصم");
                Hdr("الإجمالي");
            });

            // Data rows
            int rowIndex = 0;
            foreach (var item in _inv.Items)
            {
                bool even = rowIndex % 2 == 0;
                string bg = even ? C_RowEven : C_RowOdd;
                rowIndex++;

                const float bBtm = 0.25f;

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                    .Text(rowIndex.ToString()).FontSize(F_Sm).FontColor(C_TextLight);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4)
                    .Text(item.ItemCode ?? "—").FontSize(F_Sm).FontColor(C_TextLight);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4)
                    .Text(item.ItemName).FontSize(F_Sm);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                    .Text(item.UnitName ?? "—").FontSize(F_Sm);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                    .Text(item.ExpiryDate.HasValue ? item.ExpiryDate.Value.ToString("MM/yyyy") : "—").FontSize(F_Sm);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                    .Text($"{item.Quantity:N2}").FontSize(F_Sm);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignRight()
                    .Text($"{item.PurchasePrice:N2}").FontSize(F_Sm);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                    .Text("—").FontSize(F_Sm).FontColor(C_TextLight);

                table.Cell().Background(bg).BorderBottom(bBtm).BorderColor(C_Border).PaddingVertical(4).PaddingHorizontal(4).AlignRight()
                    .Text($"{item.LineTotal:N2}").FontSize(F_Base).SemiBold();
            }
        });
    }

    // ── TOTALS ─────────────────────────────────────────────────────────────────
    private void ComposeTotals(IContainer c)
    {
        c.Width(220).Column(col =>
        {
            col.Item().Border(0.5f).BorderColor(C_InfoBorder).Background(C_TotalsBox).Padding(10).Column(inner =>
            {
                TotalRow(inner, "المجموع الجزئي:", _co.FormatAmount(_inv.Subtotal));

                if (_inv.Discount > 0)
                    TotalRow(inner, "الخصم:", $"- {_co.FormatAmount(_inv.Discount)}");

                inner.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(C_Border);
                inner.Item().PaddingTop(4).Background(C_GrandBg).Padding(8).Row(r =>
                {
                    r.RelativeItem().Text("صافي الإجمالي:").FontSize(F_Lg).Bold().FontColor(C_GrandFg);
                    r.AutoItem().Text(_co.FormatAmount(_inv.NetTotal)).FontSize(F_Lg).Bold().FontColor(C_GrandFg);
                });
            });
        });
    }

    // ── SIGNATURES ─────────────────────────────────────────────────────────────
    private static void ComposeSignatures(IContainer c)
    {
        c.Column(col =>
        {
            // Top row: 3 internal signatures
            col.Item().PaddingBottom(8).Row(row =>
            {
                SignBox(row.RelativeItem(), "أعدّه");
                row.ConstantItem(8);
                SignBox(row.RelativeItem(), "راجعه");
                row.ConstantItem(8);
                SignBox(row.RelativeItem(), "اعتمده");
            });

            // Bottom row: supplier + stamp (right-aligned)
            col.Item().Row(row =>
            {
                row.RelativeItem();
                SignBox(row.ConstantItem(120), "توقيع المورد");
                row.ConstantItem(8);
                StampBox(row.ConstantItem(100));
            });
        });
    }

    private static void SignBox(IContainer c, string label)
    {
        c.Border(0.5f).BorderColor(C_SignBorder).Background(C_SignBg).Padding(8).Column(col =>
        {
            col.Item().AlignCenter().Text(label).FontSize(F_Sm).SemiBold().FontColor(C_TextMid);
            col.Item().PaddingTop(28).LineHorizontal(0.5f).LineColor(C_SignBorder);
        });
    }

    private static void StampBox(IContainer c)
    {
        c.Border(0.5f).BorderColor(C_SignBorder).Background(C_SignBg).Padding(8).Column(col =>
        {
            col.Item().AlignCenter().Text("الختم الرسمي").FontSize(F_Sm).SemiBold().FontColor(C_TextMid);
            col.Item().Height(36);
        });
    }

    // ── TERMS ──────────────────────────────────────────────────────────────────
    private void ComposeTerms(IContainer c)
    {
        c.Border(0.5f).BorderColor(C_TermsBorder).Background(C_TermsBg).Padding(10).Column(col =>
        {
            SectionTitle(col, "الشروط والأحكام");
            var lines = _co.TermsAndConditions!.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                col.Item().PaddingTop(2)
                    .Text($"{i + 1}. {lines[i].Trim()}").FontSize(F_Sm).FontColor(C_TextMid);
            }
        });
    }

    // ── FOOTER ─────────────────────────────────────────────────────────────────
    private void ComposeFooter(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Height(1).Background(C_Border);

            col.Item().PaddingTop(5).Row(row =>
            {
                // Bank info
                row.RelativeItem().Column(bank =>
                {
                    var hasBank = !string.IsNullOrEmpty(_co.BankName)
                               || !string.IsNullOrEmpty(_co.BankAccount)
                               || !string.IsNullOrEmpty(_co.Iban);
                    if (hasBank)
                    {
                        bank.Item().Text("المعلومات البنكية:").FontSize(F_Xs).SemiBold().FontColor(C_TextLight);
                        if (!string.IsNullOrEmpty(_co.BankName))
                            bank.Item().Text($"البنك: {_co.BankName}").FontSize(F_Xs).FontColor(C_TextLight);
                        if (!string.IsNullOrEmpty(_co.BankAccount))
                            bank.Item().Text($"الحساب: {_co.BankAccount}").FontSize(F_Xs).FontColor(C_TextLight);
                        if (!string.IsNullOrEmpty(_co.Iban))
                            bank.Item().Text($"IBAN: {_co.Iban}").FontSize(F_Xs).FontColor(C_TextLight);
                    }
                    else if (!string.IsNullOrEmpty(_co.FooterNotes))
                    {
                        bank.Item().Text(_co.FooterNotes!).FontSize(F_Xs).Italic().FontColor(C_TextLight);
                    }
                });

                // Page number
                row.AutoItem().AlignRight().Column(pg =>
                {
                    pg.Item().AlignRight().Text(x =>
                    {
                        x.Span("صفحة ").FontSize(F_Xs).FontColor(C_TextLight);
                        x.CurrentPageNumber().FontSize(F_Xs).FontColor(C_TextLight);
                        x.Span(" من ").FontSize(F_Xs).FontColor(C_TextLight);
                        x.TotalPages().FontSize(F_Xs).FontColor(C_TextLight);
                    });
                    pg.Item().AlignRight()
                        .Text($"تم الإنشاء: {DateTime.Now:dd/MM/yyyy HH:mm}  |  DentalERP")
                            .FontSize(F_Xs).FontColor(C_Label);
                });
            });
        });
    }

    // ── Shared helpers ─────────────────────────────────────────────────────────
    private static void SectionTitle(ColumnDescriptor col, string title)
    {
        col.Item().BorderBottom(1.5f).BorderColor(C_Primary).PaddingBottom(4)
            .Text(title).FontSize(F_Base).Bold().FontColor(C_Primary);
    }

    private static void MetaRow(TableDescriptor t, string label, string value)
    {
        t.Cell().PaddingVertical(2).Text(label).FontSize(F_Sm).SemiBold().FontColor(C_TextLight);
        t.Cell().PaddingVertical(2).Text(value).FontSize(F_Sm).FontColor(C_TextDark);
    }

    private static void TotalRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).FontSize(F_Sm).SemiBold().FontColor(C_TextMid);
            r.AutoItem().Text(value).FontSize(F_Sm).FontColor(C_TextDark);
        });
    }

    private static string StatusAr(string status) => status switch
    {
        "Draft"     => "مسودة",
        "Posted"    => "مرحّلة",
        "Cancelled" => "ملغاة",
        _           => status,
    };
}
