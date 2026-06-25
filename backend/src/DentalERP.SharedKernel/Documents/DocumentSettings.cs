namespace DentalERP.SharedKernel.Documents;

/// <summary>
/// Immutable snapshot of company branding settings used by every PDF document.
/// Loaded once per request via GetDocumentSettingsQuery in the IAM module.
/// </summary>
public sealed record CompanySettings
{
    public string NameAr                 { get; init; } = "عيادة الأسنان";
    public string NameEn                 { get; init; } = "Dental Clinic";
    public string? LogoUrl               { get; init; }
    public string? BusinessType          { get; init; }
    public string? Address               { get; init; }
    public string? City                  { get; init; }
    public string? Country               { get; init; }
    public string? Phone                 { get; init; }
    public string? Mobile                { get; init; }
    public string? Email                 { get; init; }
    public string? Website               { get; init; }
    public string? TaxNumber             { get; init; }
    public string? CommercialRegistration{ get; init; }
    public string? LicenseNumber         { get; init; }
    public string  Currency              { get; init; } = "LYD";
    public string  CurrencySymbol        { get; init; } = "د.ل";
    public string? FooterNotes           { get; init; }
    public string? TermsAndConditions    { get; init; }
    public string? BankName              { get; init; }
    public string? BankAccount           { get; init; }
    public string? Iban                  { get; init; }

    public string FormatAmount(decimal amount) => $"{amount:N2} {CurrencySymbol}";

    public static CompanySettings Default => new();

    public string CityCountry()
    {
        var parts = new[] { City, Country }.Where(x => !string.IsNullOrEmpty(x));
        return string.Join("، ", parts);
    }

    public string ContactLine()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(Phone))  parts.Add($"هاتف: {Phone}");
        if (!string.IsNullOrEmpty(Mobile)) parts.Add($"جوال: {Mobile}");
        if (!string.IsNullOrEmpty(Email))  parts.Add(Email!);
        if (!string.IsNullOrEmpty(Website))parts.Add(Website!);
        return string.Join("  |  ", parts);
    }
}
