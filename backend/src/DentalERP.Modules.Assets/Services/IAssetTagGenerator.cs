namespace DentalERP.Modules.Assets.Services;

internal interface IAssetTagGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
