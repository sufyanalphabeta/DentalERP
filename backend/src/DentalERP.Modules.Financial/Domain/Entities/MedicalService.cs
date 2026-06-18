namespace DentalERP.Modules.Financial.Domain.Entities;

public sealed class MedicalService
{
    public Guid Id { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private MedicalService() { }

    public static MedicalService Create(string name, decimal price, Guid? categoryId = null, string? code = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            CategoryId = categoryId,
            Code = code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, decimal price, Guid? categoryId, string? code)
    {
        Name = name;
        Price = price;
        CategoryId = categoryId;
        Code = code;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { DeletedAt = DateTime.UtcNow; IsActive = false; }
}
