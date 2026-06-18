using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.Inventory.Domain.Entities;

public sealed class ItemCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? NameAr { get; private set; }
    public Guid? ParentId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ItemCategory> _children = [];
    public IReadOnlyList<ItemCategory> Children => _children;

    private ItemCategory() { }

    public static ItemCategory Create(string name, string? nameAr = null, Guid? parentId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameAr = nameAr,
            ParentId = parentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, string? nameAr, Guid? parentId)
    {
        Name = name;
        NameAr = nameAr;
        ParentId = parentId;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }
    public void Delete()     => SoftDelete();
}
