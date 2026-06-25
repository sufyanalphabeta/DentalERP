using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Assets.Domain.Entities;

public sealed class AssetCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public string? Description { get; private set; }
    public decimal? DepreciationRate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AssetCategory() { }

    public static AssetCategory Create(string name, string? nameAr = null, string? description = null, decimal? depreciationRate = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        return new AssetCategory
        {
            Id = Guid.NewGuid(), Name = name.Trim(), NameAr = nameAr,
            Description = description, DepreciationRate = depreciationRate,
            IsActive = true, CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, string? description, decimal? depreciationRate)
    {
        Name = name.Trim(); NameAr = nameAr; Description = description;
        DepreciationRate = depreciationRate;
        Touch();
    }

    public void Delete() => SoftDelete();
}
