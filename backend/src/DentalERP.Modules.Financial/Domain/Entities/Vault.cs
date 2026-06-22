namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class Vault
{
    public static readonly string[] ValidTypes = ["cash", "bank", "card", "pos"];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public decimal OpeningBalance { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private Vault() { }

    public static Vault Create(string name, string type, decimal openingBalance = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            OpeningBalance = openingBalance,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
