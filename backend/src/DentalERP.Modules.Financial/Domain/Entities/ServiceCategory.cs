namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class ServiceCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public short SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private ServiceCategory() { }

    public static ServiceCategory Create(string name, short sortOrder = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, short sortOrder) { Name = name; SortOrder = sortOrder; }
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
