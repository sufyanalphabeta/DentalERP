namespace DentalERP.Modules.IAM.Infrastructure.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenExpiryMinutes { get; init; } = 60;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
