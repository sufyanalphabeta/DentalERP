using DentalERP.Modules.Assets.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DentalERP.Modules.Assets.Services;

internal sealed class AssetTagGenerator(AssetsDbContext db) : IAssetTagGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var seq = await db.Database
            .SqlQuery<long>($"SELECT nextval('asset_tag_seq') AS \"Value\"")
            .FirstAsync(ct);
        return $"AST-{seq:D6}";
    }
}
