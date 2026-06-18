namespace DentalERP.Modules.Radiology.Domain.Entities;

public sealed class RadiologyType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? NameAr { get; private set; }
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RadiologyType() { }

    public static RadiologyType Create(string name, string? nameAr, decimal basePrice)
    {
        return new RadiologyType
        {
            Id = Guid.NewGuid(),
            Name = name,
            NameAr = nameAr,
            BasePrice = basePrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? nameAr, decimal basePrice)
    {
        Name = name;
        NameAr = nameAr;
        BasePrice = basePrice;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
