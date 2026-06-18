using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class Warehouse : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public string? Location { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Warehouse() { }

    public static Warehouse Create(string name, string? nameAr = null, string? location = null, bool isDefault = false)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameAr = nameAr,
            Location = location,
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, string? nameAr, string? location)
    {
        Name = name;
        NameAr = nameAr;
        Location = location;
        Touch();
    }

    public void SetAsDefault()  { IsDefault = true;  Touch(); }
    public void Deactivate()    { IsActive = false;   Touch(); }
    public void Delete()        => SoftDelete();
}
