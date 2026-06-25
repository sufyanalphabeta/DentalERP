using DentalERP.Modules.IAM.Infrastructure;
using DentalERP.SharedKernel.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.IAM.Features.Settings;

// The query record lives in SharedKernel so every module can send it.
// Only the handler lives here in IAM, which owns the system_settings table.
public sealed class GetCompanySettingsQueryHandler(IAMDbContext db)
    : IRequestHandler<GetCompanySettingsQuery, CompanySettings>
{
    public async Task<CompanySettings> Handle(GetCompanySettingsQuery request, CancellationToken ct)
    {
        var raw = await db.SystemSettings
            .AsNoTracking()
            .Where(s => s.Group == "company")
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        string Get(string key, string def = "") =>
            raw.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : def;
        string? Opt(string key) =>
            raw.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

        return new CompanySettings
        {
            NameAr                  = Get("company.nameAr",  "عيادة الأسنان"),
            NameEn                  = Get("company.nameEn",  "Dental Clinic"),
            LogoUrl                 = Opt("company.logoUrl"),
            BusinessType            = Opt("company.businessType"),
            Address                 = Opt("company.address"),
            City                    = Opt("company.city"),
            Country                 = Opt("company.country"),
            Phone                   = Opt("company.phone"),
            Mobile                  = Opt("company.mobile"),
            Email                   = Opt("company.email"),
            Website                 = Opt("company.website"),
            TaxNumber               = Opt("company.taxNumber"),
            CommercialRegistration  = Opt("company.commercialRegistration"),
            LicenseNumber           = Opt("company.licenseNumber"),
            Currency                = Get("company.currency",       "LYD"),
            CurrencySymbol          = Get("company.currencySymbol", "د.ل"),
            FooterNotes             = Opt("company.footerNotes"),
            TermsAndConditions      = Opt("company.termsAndConditions"),
            BankName                = Opt("company.bankName"),
            BankAccount             = Opt("company.bankAccount"),
            Iban                    = Opt("company.iban"),
        };
    }
}
